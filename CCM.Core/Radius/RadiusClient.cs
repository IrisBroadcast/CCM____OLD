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

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FP.Radius;

namespace CCM.Core.Radius
{
    /*
 * This code is a copy of
https://github.com/frontporch/Radius.NET/blob/master/Radius/RadiusClient.cs
* with one minor change to avoid a problem that occured.
* TODO: Find out what kind of problem this is and if it's fixed in later versions of the package
*/
    public class RadiusClient
    {
        #region Constants
        private const int DEFAULT_RETRIES = 3;
        private const uint DEFAULT_AUTH_PORT = 1812;
        private const uint DEFAULT_ACCT_PORT = 1813;
        private const int DEFAULT_SOCKET_TIMEOUT = 3000;
        #endregion

        #region Private
        private string _SharedSecret = String.Empty;
        private string _HostName = String.Empty;
        private uint _AuthPort = DEFAULT_AUTH_PORT;
        private uint _AcctPort = DEFAULT_ACCT_PORT;
        private uint _AuthRetries = DEFAULT_RETRIES;
        private uint _AcctRetries = DEFAULT_RETRIES;
        private int _SocketTimeout = DEFAULT_SOCKET_TIMEOUT;
        #endregion

        #region Properties
        public int SocketTimeout
        {
            get { return _SocketTimeout; }
            set { _SocketTimeout = value; }
        }
        #endregion

        #region Constructors
        public RadiusClient(string hostName, string sharedSecret, int sockTimeout = DEFAULT_SOCKET_TIMEOUT, uint authPort = DEFAULT_AUTH_PORT, uint acctPort = DEFAULT_ACCT_PORT)
        {
            _HostName = hostName;
            _AuthPort = authPort;
            _AcctPort = acctPort;
            _SharedSecret = sharedSecret;
            _SocketTimeout = sockTimeout;
        }
        #endregion

        #region Public Methods
        public RadiusPacket Authenticate(string username, string password)
        {
            RadiusPacket packet = new RadiusPacket(RadiusCode.ACCESS_REQUEST);
            packet.SetAuthenticator(_SharedSecret);
            byte[] encryptedPass = Utils.EncodePapPassword(Encoding.ASCII.GetBytes(password), packet.Authenticator, _SharedSecret);
            packet.SetAttribute(new RadiusAttribute(RadiusAttributeType.USER_NAME, Encoding.ASCII.GetBytes(username)));
            packet.SetAttribute(new RadiusAttribute(RadiusAttributeType.USER_PASSWORD, encryptedPass));
            return packet;
        }

        public async Task<RadiusPacket> SendAndReceivePacket(RadiusPacket packet, int retries = DEFAULT_RETRIES)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _SocketTimeout);

                try
                {
                    IPAddress hostIP;

                    if (IPAddress.TryParse(_HostName, out hostIP))
                        udpClient.Connect(hostIP, (int)_AuthPort);
                    else
                        udpClient.Connect(_HostName, (int)_AuthPort);

                }
                catch (SocketException e)
                {
                    int hr = Marshal.GetHRForException(e);
                    string hexValue = hr.ToString("X");

                    //The requested name is valid, but no data of the requested type was found
                    if (hexValue == "80004005")
                        return null;
                }

                var endPoint = (IPEndPoint)udpClient.Client.RemoteEndPoint;

                int numberOfAttempts = 0;

                do
                {
                    // Ändring här:
                    //await udpClient.SendAsync(packet.RawData, packet.RawData.Length);
                    udpClient.Send(packet.RawData, packet.RawData.Length);

                    try
                    {
                        // Using the synchronous method for the timeout features
                        var result = udpClient.Receive(ref endPoint);
                        RadiusPacket receivedPacket = new RadiusPacket(result);

                        if (receivedPacket.Valid && VerifyAuthenticator(packet, receivedPacket))
                            return receivedPacket;
                    }
                    catch (SocketException)
                    {
                        //Server isn't responding
                    }

                    numberOfAttempts++;

                } while (numberOfAttempts < retries);
            }

            return null;
        }
        #endregion

        #region Private Methods
        private bool VerifyAuthenticator(RadiusPacket requestedPacket, RadiusPacket receivedPacket)
        {
            return requestedPacket.Identifier == receivedPacket.Identifier
                && receivedPacket.Authenticator.SequenceEqual(Utils.ResponseAuthenticator(receivedPacket.RawData, requestedPacket.Authenticator, _SharedSecret));
        }

        public static bool VerifyAccountingAuthenticator(byte[] radiusPacket, string secret)
        {
            var secretBytes = Encoding.ASCII.GetBytes(secret);

            byte[] sum = new byte[radiusPacket.Length + secretBytes.Length];

            byte[] authenticator = new byte[16];
            Array.Copy(radiusPacket, 4, authenticator, 0, 16);

            Array.Copy(radiusPacket, 0, sum, 0, radiusPacket.Length);
            Array.Copy(secretBytes, 0, sum, radiusPacket.Length, secretBytes.Length);
            Array.Clear(sum, 4, 16);

            MD5 md5 = new MD5CryptoServiceProvider();

            var hash = md5.ComputeHash(sum, 0, sum.Length);
            return authenticator.SequenceEqual(hash);
        }
        #endregion
    }
}