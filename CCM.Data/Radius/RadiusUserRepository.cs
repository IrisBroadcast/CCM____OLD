/*
 * Copyright (c) 2017 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using NLog;
using MySql.Data.MySqlClient;

namespace CCM.Data.Radius
{
    /// <summary>
    /// Account management. Connects to and updates data directly in the MySql database of the Radius server.
    /// </summary>
    public class RadiusUserRepository : IRadiusUserRepository
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static MySqlConnection GetMySqlConnection()
        {
            return new MySqlConnection(ConfigurationManager.ConnectionStrings["RadiusDbContext"].ConnectionString);
        }

        public bool UserExists(string userName)
        {
            log.Debug("Checking if user {0} exists in RADIUS", userName);
            try
            {
                using (var conn = GetMySqlConnection())
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM radcheck WHERE username=@username";
                    cmd.Parameters.AddWithValue("@username", userName);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    log.Debug("Result from RADIUS: User name count = {0}", count);
                    return count > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error when checking for existing user", ex);
                return false; // Actually, we don't know.
            }
        }


        /// <summary>
        /// Returns the id of the newly created user, or -1 if failed.
        /// </summary>
        public long CreateUser(string userName, string password, UserType userType)
        {
            log.Debug("RadiusUserRepository.CreateUser called. UserName:{0}, UserType:{1}", userName, userType);

            using (var conn = GetMySqlConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.Parameters.AddWithValue("@username", userName);

                if (userType == UserType.CcmUser)
                {
                    cmd.CommandText = "INSERT INTO radcheck (username, attribute, op, value) values (@username, 'SMD5-Password', ':=', @password)";
                    cmd.Parameters.AddWithValue("@password", CryptoHelper.Md5HashSaltedPassword(password));
                }
                else
                {
                    cmd.CommandText = "INSERT INTO radcheck (username, attribute, op, value) values (@username, 'Cleartext-Password', ':=', @password)";
                    cmd.Parameters.AddWithValue("@password", password);
                }

                try
                {
                    cmd.ExecuteNonQuery();
                    long id = cmd.LastInsertedId;
                    return id;
                }
                catch (Exception ex)
                {
                    log.Error("Error creating RADIUS user", ex);
                    return -1;
                }
            }
        }

        public bool UpdatePassword(long userId, string password, UserType userType)
        {
            log.Debug("RadiusUserRepository.UpdatePassword called. UserId:{0}, UserType:{1}", userId, userType);

            using (var conn = GetMySqlConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                if (userType == UserType.CcmUser)
                {
                    cmd.CommandText = "UPDATE radcheck SET value = @password, attribute = 'SMD5-Password' WHERE id = @userId";
                    cmd.Parameters.AddWithValue("@password", CryptoHelper.Md5HashSaltedPassword(password));
                }
                else
                {
                    cmd.CommandText = "UPDATE radcheck SET value = @password, attribute = 'Cleartext-Password' WHERE id = @userId";
                    cmd.Parameters.AddWithValue("@password", password);
                }

                cmd.Parameters.AddWithValue("@userId", userId);

                int result = cmd.ExecuteNonQuery();

                return result > 0;
            }
        }

        public bool ChangeUserName(long userId, string userName)
        {
            log.Debug("RadiusUserRepository.ChangeUserName called. userId:{0}, userName:{1}", userId, userName);

            using (var conn = GetMySqlConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                // Kolla om användarnamnet är upptaget
                cmd.CommandText = "SELECT COUNT(*) FROM radcheck WHERE username=@username";
                cmd.Parameters.AddWithValue("@username", userName);

                try
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                cmd.CommandText = "UPDATE radcheck SET username = @username WHERE id = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);

                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }

        public bool DeleteUser(long userId)
        {
            log.Debug("RadiusUserRepository.DeleteUser called. userId:{0}", userId);

            using (var conn = GetMySqlConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = "DELETE FROM radcheck WHERE id = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "select count(*) as u FROM radcheck WHERE id = @userId";
                int result = Convert.ToInt32(cmd.ExecuteScalar());

                return result == 0;
            }
        }

        public List<RadiusUser> GetUsers()
        {
            log.Debug("RadiusUserRepository.GetUsers called.");

            var users = new List<RadiusUser>();

            using (var conn = GetMySqlConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = " select id, username from radcheck";

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var user = new RadiusUser
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString()
                    };

                    users.Add(user);
                }
            }

            log.Debug("Found {0} radius users.", users.Count);
            return users;
        }

        public bool Unlock(long userId, string userName)
        {
            log.Debug("RadiusUserRepository.Unlock called. userId:{0}, userName:{1}", userId, userName);

            using (var conn = GetMySqlConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT COUNT(*) FROM radcheck WHERE username=@username";
                cmd.Parameters.AddWithValue("@username", userName);

                try
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                cmd.CommandText = "UPDATE radcheck SET username = @username WHERE id = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);

                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }

        /// <summary>
        /// Locks the specified user.
        /// </summary>
        /// <remarks>
        /// The account is locked by setting the username to "locked".
        /// This is a dubious solution. The user can still log in with current password and username "locked".
        /// TODO: Change this implementation
        /// </remarks>
        public bool Lock(long userId)
        {
            log.Debug("RadiusUserRepository.Lock called. userId:{0}", userId);

            using (var conn = GetMySqlConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = "UPDATE radcheck SET username = 'locked' WHERE id = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);

                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }
    }
}