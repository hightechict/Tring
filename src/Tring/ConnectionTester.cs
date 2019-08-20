//Copyright 2019 Hightech ICT and authors

//This file is part of Tring.

//Tring is free software: you can redistribute it and/or modify
//it under the terms of the GNU Lesser General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//Tring is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU Lesser General Public License for more details.

//You should have received a copy of the GNU Lesser General Public License
//along with Tring.If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Tring
{
    internal class ConnectionTester
    {
        private static readonly Regex checkIfIp = new Regex(@"^(?<ip>\d+\.\d+\.\d+\.\d+)(\:(?<port>.+))?$", RegexOptions.Compiled);
        private static readonly Regex checkIfUrl = new Regex(@"^((?<protocol>http|https|ftp)\:\/\/)*(?<host>[\w\.\-~]+(\.[\w\.\-~]+)*)(\:(?<port>\w+))?(\/.+)*", RegexOptions.Compiled);
        private readonly TimeSpan waitTime = TimeSpan.FromSeconds(1);

        public enum ConnectionStatus { Succes, TimeOut, Refused, Untried };

        public ConnectionRequest request;

        public ConnectionTester(string UrlOrIp, string port = "")
        {
            if(!CreateRequestIfIp(UrlOrIp,out request))
            {
                if (!CreateRequestIfURL(UrlOrIp, out request))
                {
                    throw new ArgumentException($"Input: {UrlOrIp} is nether a ip nor a url.");
                }
            }

            if (!string.IsNullOrEmpty(port))
            {
                var convertedPort = PortLogic.StringToPort(port);
                request = new ConnectionRequest(request.Ip, convertedPort,request.Url);
            }
            if (request.Port == PortLogic.UnsetPort) throw new ArgumentException($"The input you provided for the port is not valid, your input: {port}");
        }

        public ConnectionResult TryConnect()
        {
            ConnectionStatus Connection, DNS;
            DNS = ConnectionStatus.Untried;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result;
            if (string.IsNullOrEmpty(request.Ip))
            {
                DNS = DnsLookup(request.Url, out var ip);
                request = new ConnectionRequest(ip, request.Port, request.Url);
                if (DNS != ConnectionStatus.Succes)
                    return new ConnectionResult(request,DNS);
            }
            var watch = System.Diagnostics.Stopwatch.StartNew();
            result = socket.BeginConnect(request.Ip, request.Port, null, null);
            bool connectionSuccess = result.AsyncWaitHandle.WaitOne(waitTime);
            var localInterface = GetLocalPath(request.Ip, socket);
            if (socket.Connected)
            {
                watch.Stop();
                socket.EndConnect(result);
                var connectionTimeMs = watch.ElapsedMilliseconds;
                Connection = ConnectionStatus.Succes;
                return new ConnectionResult(request, DNS,Connection,ConnectionStatus.Untried,localInterface,connectionTimeMs);
            }
            else
            {
                socket.Close();
                var (Ping, PingTimeMs) = PingHost(request.Ip);
                if (connectionSuccess)
                    Connection = ConnectionStatus.Refused;
                else
                    Connection = ConnectionStatus.TimeOut;
                return new ConnectionResult(request, DNS, Connection, Ping, localInterface, 0, PingTimeMs);
            }
        }
        public static (ConnectionStatus status, long timeInMs) PingHost(string nameOrAddress)
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send(nameOrAddress);
                return (reply?.Status == IPStatus.Success ? ConnectionStatus.Succes : ConnectionStatus.TimeOut, reply?.RoundtripTime ?? long.MinValue);
            }
        }

        private ConnectionStatus DnsLookup(string host,out string ip)
        {
            ip = "";
            IAsyncResult lookupResult = Dns.BeginGetHostEntry(host, null, null);
            bool DnsLookupSucces = lookupResult.AsyncWaitHandle.WaitOne(waitTime);
            if (DnsLookupSucces)
            {
                try
                {
                    ip = Dns.EndGetHostEntry(lookupResult)?.AddressList[0]?.ToString();
                    return ConnectionStatus.Succes;
                }
                catch (Exception)
                {
                    return ConnectionStatus.Refused;
                }
            }
            else
            {
                return ConnectionStatus.TimeOut;
            }
        }

        private static bool CreateRequestIfIp(string toCheck,out ConnectionRequest request)
        {
            request = new ConnectionRequest();
            var port = PortLogic.UnsetPort;
            var ipMatch = checkIfIp.Match(toCheck);
            if (ipMatch.Success)
            {
                var host = ipMatch.Groups["ip"].Value;
                if (!string.IsNullOrEmpty(ipMatch.Groups["port"].Value))
                {
                    port = PortLogic.StringToPort(ipMatch.Groups["port"].Value);
                    if (port == PortLogic.UnsetPort)
                    {
                        port = PortLogic.DeterminePortByProtocol(ipMatch.Groups["port"].Value);
                    }
                }
                request = new ConnectionRequest(host, port);
                return true;
            }
            return false;
        }

        private static bool CreateRequestIfURL(string toCheck, out ConnectionRequest request)
        {
            request = new ConnectionRequest();
            var port = PortLogic.UnsetPort;
            var urlMatch = checkIfUrl.Match(toCheck);
            if (urlMatch.Success)
            {
                var url = urlMatch.Groups["host"].Value;
                if (!string.IsNullOrEmpty(urlMatch.Groups["port"].Value))
                {
                    port = PortLogic.StringToPort(urlMatch.Groups["port"].Value);
                    if (port == PortLogic.UnsetPort)
                    {
                        port = PortLogic.DeterminePortByProtocol(urlMatch.Groups["port"].Value);
                    }
                }
                else if (port == PortLogic.UnsetPort && !string.IsNullOrEmpty(urlMatch.Groups["protocol"].Value))
                {
                    port = PortLogic.DeterminePortByProtocol(urlMatch.Groups["protocol"].Value);
                }
                request = new ConnectionRequest("", port,url);
                return true;
            }
            return false;
        }

        private static string GetLocalPath(string ip, Socket socket)
        {
            IPAddress remoteIp = IPAddress.Parse(ip);
            IPEndPoint remoteEndPoint = new IPEndPoint(remoteIp, 0);
            IPEndPoint localEndPoint = QueryRoutingInterface(socket, remoteEndPoint);
            return localEndPoint.Address.ToString();
        }

        private static IPEndPoint QueryRoutingInterface(
          Socket socket,
          IPEndPoint remoteEndPoint)
        {
            SocketAddress address = remoteEndPoint.Serialize();

            byte[] remoteAddrBytes = new byte[address.Size];
            for (int i = 0; i < address.Size; i++)
            {
                remoteAddrBytes[i] = address[i];
            }

            byte[] outBytes = new byte[remoteAddrBytes.Length];
            socket.IOControl(
                        IOControlCode.RoutingInterfaceQuery,
                        remoteAddrBytes,
                        outBytes);
            for (int i = 0; i < address.Size; i++)
            {
                address[i] = outBytes[i];
            }

            EndPoint ep = remoteEndPoint.Create(address);
            return (IPEndPoint)ep;
        }
    }
}
