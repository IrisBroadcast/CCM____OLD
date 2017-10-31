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
using System.Net;
using System.Net.Sockets;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Commands;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Commands.Base;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Enums;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Responses;
using CCM.Core.CodecControl.Entities;
using CCM.Core.CodecControl.Enums;
using CCM.Core.Exceptions;
using NLog;
using CCM.CodecControl.Helpers;

namespace CCM.CodecControl.Prodys.IkusNet
{
    public class IkusNetApi : ICodecApi
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public bool CheckIfAvailable(string ip)
        {
            try
            {
                using (var socket = GetConnectedSocket(ip))
                {
                    socket.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Get Commands

        public string GetDeviceName(string hostAddress)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetSysGetDeviceName());
                var response = new IkusNetGetDeviceNameResponse(socket);
                return response.DeviceName;
            }
        }

        public bool? GetGpi(string hostAddress, int gpio)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetGpi { Gpio = gpio });
                var response = new IkusNetGetGpiResponse(socket);
                return response.Active;
            }
        }

        public bool? GetGpo(string hostAddress, int gpio)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetGpo { Gpio = gpio });
                var response = new IkusNetGetGpoResponse(socket);
                return response.Active;
            }
        }

        public bool GetInputEnabled(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetInputEnabled { Input = input });
                var response = new IkusNetGetInputEnabledResponse(socket);
                return response.Enabled;
            }
        }

        public int GetInputGainLevel(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetInputGainLevel { Input = input });
                var response = new IkusNetGetInputGainLevelResponse(socket);
                return response.GainLeveldB;
            }
        }

        public LineStatus GetLineStatus(string hostAddress, int line)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetLineStatus { Line = (IkusNetLine)line });
                var response = new IkusNetGetLineStatusResponse(socket);

                return new LineStatus
                {
                    RemoteAddress = response.Address,
                    StatusCode = (LineStatusCode)response.LineStatus,
                    DisconnectReason = (DisconnectReason)response.DisconnectionCode,
                    IpCallType = (IpCallType)response.IpCallType
                };
            }
        }

        public string GetLoadedPresetName(string hostAddress, string lastPresetName)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetLoadedPresetName { LastLoadedPresetName = lastPresetName });
                var response = new IkusNetGetLoadedPresetNameResponse(socket);
                return response.PresetName;
            }
        }

        public VuValues GetVuValues(string hostAddress)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetVuMeters());
                var response = new IkusNetGetVumetersResponse(socket);
                return new VuValues
                {
                    TxLeft = response.ProgramTxLeft,
                    TxRight = response.ProgramTxRight,
                    RxLeft = response.ProgramRxLeft,
                    RxRight = response.ProgramRxRight
                };
            }
        }

        public AudioMode GetAudioMode(string hostAddress)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                // Get encoder algoritm
                SendCommand(socket, new CommandIkusNetGetEncoderAudioMode());
                var encoderResponse = IkusNetGetEncoderAudioModeResponse.GetResponse(socket);

                // Get decoder algoritm
                SendCommand(socket, new CommandIkusNetGetDecoderAudioMode());
                var decoderResponse = IkusNetGetDecoderAudioModeResponse.GetResponse(socket);

                return new AudioMode
                {
                    EncoderAudioAlgoritm = (AudioAlgorithm)encoderResponse.AudioAlgorithm,
                    DecoderAudioAlgoritm = (AudioAlgorithm)decoderResponse.AudioAlgorithm
                };
            }
        }

        public AudioStatus GetAudioStatus(string hostAddress, int nrOfInputs, int nrOfGpos)
        {
            var audioStatus = new AudioStatus();

            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetVuMeters());
                var vuResponse = new IkusNetGetVumetersResponse(socket);

                audioStatus.VuValues = new VuValues
                {
                    TxLeft = vuResponse.ProgramTxLeft,
                    TxRight = vuResponse.ProgramTxRight,
                    RxLeft = vuResponse.ProgramRxLeft,
                    RxRight = vuResponse.ProgramRxRight
                };

                audioStatus.InputStatuses = new List<InputStatus>();

                for (int input = 0; input < nrOfInputs; input++)
                {
                    SendCommand(socket, new CommandIkusNetGetInputEnabled { Input = input });
                    var enabledResponse = new IkusNetGetInputEnabledResponse(socket);
                    var inputEnabled = enabledResponse.Enabled;

                    SendCommand(socket, new CommandIkusNetGetInputGainLevel { Input = input });
                    var gainLevelResponse = new IkusNetGetInputGainLevelResponse(socket);
                    var gainLevel = gainLevelResponse.GainLeveldB;

                    audioStatus.InputStatuses.Add(new InputStatus { Enabled = inputEnabled, GainLevel = gainLevel });
                }

                //audioStatus.Gpis = new List<bool>();

                //for (int gpi = 0; gpi < nrOfGpis; gpi++)
                //{
                //    SendCommand(socket, new CommandIkusNetGetGpi { Gpio = gpi });
                //    var response = new IkusNetGetGpiResponse(socket);
                //    var gpiEnabled = response.Active;
                //    if (!gpiEnabled.HasValue)
                //    {
                //        // Indication of missing GPI for the number. Probably we passed the last one.
                //        break;
                //    }
                //    audioStatus.Gpis.Add(gpiEnabled.Value);
                //}

                audioStatus.Gpos = new List<bool>();

                for (int gpo = 0; gpo < nrOfGpos; gpo++)
                {
                    SendCommand(socket, new CommandIkusNetGetGpo { Gpio = gpo });
                    var response = new IkusNetGetGpoResponse(socket);
                    var gpoEnable = response.Active;
                    if (!gpoEnable.HasValue)
                    {
                        // Indication of missing GPO for the number. Probably we passed the last one.
                        break;
                    }
                    audioStatus.Gpos.Add(gpoEnable.Value);
                }
            }

            return audioStatus;
        }

        #endregion

        #region Configuration Commands
        public bool Call(string hostAddress, Call call)
        {
            var cmd = new CommandIkusNetCall
            {
                Address = call.Address,
                CallContent = (IkusNetCallContent)call.Content,
                CallType = (IkusNetIPCallType)call.CallType,
                Codec = (IkusNetCodec)call.Codec,
                Profile = call.Profile
            };
            return SendConfigurationCommand(hostAddress, cmd);
        }

        public bool HangUp(string hostAddress, Codec codec)
        {
            var cmd = new CommandIkusNetHangUp { Codec = (IkusNetCodec)codec };
            return SendConfigurationCommand(hostAddress, cmd);
        }

        public bool LoadPreset(string hostAddress, string preset)
        {
            var cmd = new CommandIkusNetPresetLoad { PresetToLoad = preset };
            return SendConfigurationCommand(hostAddress, cmd);
        }

        public bool Reboot(string hostAddress)
        {
            var cmd = new CommandIkusNetReboot();
            return SendConfigurationCommand(hostAddress, cmd);
        }

        public bool SetDeviceName(string hostAddress, string newDeviceName)
        {
            var cmd = new CommandIkusNetSysSetDeviceName { DeviceName = newDeviceName };
            return SendConfigurationCommand(hostAddress, cmd);
        }

        public bool SetGpo(string hostAddress, int gpo, bool active)
        {
            var cmd = new CommandIkusNetSetGpo { Active = active, Gpo = gpo };
            return SendConfigurationCommand(hostAddress, cmd);
        }

        public bool SetInputEnabled(string hostAddress, int input, bool enabled)
        {
            // Fungerar endast på Quantum-kodare, ej Quantum ST
            var cmd = new CommandIkusNetSetInputEnabled { Input = input, Enabled = enabled };
            return SendConfigurationCommand(hostAddress, cmd);
        }

        public bool SetInputGainLevel(string hostAddress, int input, int gainLevel)
        {
            // Fungerar endast på Quantum-kodare, ej Quantum ST
            var cmd = new CommandIkusNetSetInputGainLevel { GainLeveldB = gainLevel, Input = input };
            return SendConfigurationCommand(hostAddress, cmd);
        }
        #endregion

        #region Private methods

        private bool SendConfigurationCommand(string hostAddress, ICommandBase cmd)
        {
            using (var socket = GetConnectedSocket(hostAddress))
            {
                SendCommand(socket, cmd);
                var ackResponse = new AcknowledgeResponse(socket);
                return ackResponse.Acknowleged;
            }
        }

        private int SendCommand(Socket socket, ICommandBase command)
        {
            return socket.Send(command.GetBytes());
        }

        private Socket GetConnectedSocket(string address, int sendTimeout = 300)
        {
            IPAddress ipAddress = GetIpAddress(address);
            if (ipAddress == null)
            {
                throw new UnableToResolveAddressException(string.Format("Unable to resolve ip address for {0}", address));
            }

            // Try with authenticated connect first
            // INFO: It seems that authenticated connect works also when authentication is not active on the codec. At least on some firmware versions...
            Socket connectedSocket = Connect(ipAddress, new CsConnect2(), sendTimeout);

            if (connectedSocket != null)
            {
                return connectedSocket;
            }

            log.Warn("Unable to connect to codec at {0} using authenticated connect.", ipAddress);

            // Otherwise, try non authenticated connect
            connectedSocket = Connect(ipAddress, new CsConnect(), sendTimeout);

            if (connectedSocket != null)
            {
                return connectedSocket;
            }

            log.Warn("Unable to connect to codec at {0}. Both authenticated and unauthenticated connect failed.", ipAddress);
            throw new UnableToConnectException();
        }

        private IPAddress GetIpAddress(string address)
        {
            IPAddress ipAddress;

            if (IPAddress.TryParse(address, out ipAddress))
            {
                return ipAddress;
            }

            var ips = Dns.GetHostAddresses(address);
            if (ips != null && ips.Length > 0)
            {
                return ips[0];
            }

            return null;
        }

        private Socket Connect(IPAddress ipAddress, ConnectCommandBase connectCmd, int sendTimeout)
        {
            Socket socket = null;

            try
            {
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.IP);

                if (sendTimeout > 0)
                {
                    socket.SendTimeout = sendTimeout;
                }

                var endpoint = new IPEndPoint(ipAddress, Sdk.IkusNet.ExternalProtocolIpCommandsPort);
                socket.Connect(endpoint, TimeSpan.FromMilliseconds(1000));

                if (!socket.Connected)
                {
                    socket.Close();
                    return null;
                }

                var sent = SendCommand(socket, connectCmd);

                if (sent <= 0 || !socket.Connected)
                {
                    socket.Close();
                    return null;
                }

                var ackResponse = new AcknowledgeResponse(socket);
                log.Debug("Connect response from codec at {0}: {1}", ipAddress, ackResponse);

                var success = ackResponse.Acknowleged;

                if (!success)
                {
                    socket.Close();
                    return null;
                }

                return socket;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when connecting to codec at {0}", ipAddress);
                socket?.Close();
                return null;
            }
        }

        #endregion


    }
}