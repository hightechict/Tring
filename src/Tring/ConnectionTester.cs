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
using System.Linq;
using System.Text.RegularExpressions;

namespace Tring
{
    internal class ConnectionTester
    {
        private readonly TimeSpan waitTime = TimeSpan.FromSeconds(1);
        private bool IPv6Mode;
        
        public static readonly Regex SplitFormat = new Regex(@"^(?<host>.*):(?<port>\w+)$");
        public enum ConnectionStatus { Succes, TimeOut, Refused, Untried };
        public ConnectionRequest request;

        public ConnectionTester(string input,bool IPv6Mode)
        {
            this.IPv6Mode = IPv6Mode;
            request = new ConnectionRequest();
            var match = SplitFormat.Match(input);
            var uriCreated = Uri.TryCreate(input, UriKind.Absolute, out var uri);
            if(uriCreated && !string.IsNullOrEmpty(uri.DnsSafeHost))
            {
                request = new ConnectionRequest(null, uri.Port, uri.DnsSafeHost);
            }
            else if (match.Success)
            {
                var convertedPort = PortLogic.StringToPort(match.Groups["port"].Value);
                if (convertedPort == PortLogic.UnsetPort)
                    convertedPort = PortLogic.DeterminePortByProtocol(match.Groups["port"].Value);
                if (!IPAddress.TryParse(match.Groups["host"].Value, out var ip))
                    request = new ConnectionRequest(null, convertedPort, match.Groups["host"].Value);
                else
                    request = new ConnectionRequest(ip, convertedPort);

                if (IPv6Mode && request.Ip.AddressFamily != AddressFamily.InterNetworkV6)
                    throw new InvalidOperationException($"IPv6 mode is active but the ip provided is not IPv6: '{request.Ip.ToString()}'");

                if (request.Port == PortLogic.UnsetPort || request.Port < 0 || request.Port > ushort.MaxValue)
                    throw new ArgumentException($"The input you provided for the port is not valid, your input: {request.Port}.");
            }
            else
            {
                throw new ArgumentException($"Invalid input: {input} is nether a valid url nor a host:port or host:protocol.");
            }
        }

        public ConnectionResult TryConnect()
        {
            ConnectionStatus Connection, DNS;
            DNS = ConnectionStatus.Untried;
            Socket socket;
            if (IsIPv6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            }
            else
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result;
            if (request.Ip == null)
            {
                DNS = DnsLookup(request.Url, out var ip);
                request = new ConnectionRequest(ip, request.Port, request.Url);
                if (DNS != ConnectionStatus.Succes)
                    return new ConnectionResult(request, DNS);
            }
            var watch = System.Diagnostics.Stopwatch.StartNew();
            result = socket.BeginConnect(request.Ip, request.Port, null, null);
            var connectionSuccess = result.AsyncWaitHandle.WaitOne(waitTime);
            var localInterface = GetLocalPath(request.Ip, socket); 
            if (socket.Connected)
            {
                watch.Stop();
                socket.EndConnect(result);
                var connectionTimeMs = watch.ElapsedMilliseconds;
                Connection = ConnectionStatus.Succes;
                return new ConnectionResult(request, DNS, Connection, ConnectionStatus.Untried, localInterface, connectionTimeMs);
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
        public static (ConnectionStatus status, long timeInMs) PingHost(IPAddress ip)
        {
            using (var ping = new Ping())
            {
                PingReply reply;
                try
                {
                    reply = ping.Send(ip);
                }
                catch(Exception)
                {
                    return (ConnectionStatus.Refused, 0);
                }
                return (reply?.Status == IPStatus.Success ? ConnectionStatus.Succes : ConnectionStatus.TimeOut, reply?.RoundtripTime ?? long.MinValue);
            }
        }

        private ConnectionStatus DnsLookup(string host, out IPAddress ip)
        {
            ip = null;
            IAsyncResult lookupResult = Dns.BeginGetHostEntry(host, null, null);
            bool DnsLookupSucces = lookupResult.AsyncWaitHandle.WaitOne(waitTime);
            if (DnsLookupSucces)
            {
                try
                {
                    if(IPv6Mode)
                    {
                        ip = Dns.EndGetHostEntry(lookupResult)?.AddressList.FirstOrDefault(foundIp => foundIp.AddressFamily == AddressFamily.InterNetworkV6);
                        if (ip == null)
                            throw new ArgumentException($"No IPv6 address was found for {host}.");
                    }
                    else
                    {
                        ip = Dns.EndGetHostEntry(lookupResult)?.AddressList.FirstOrDefault(foundIp => foundIp.AddressFamily == AddressFamily.InterNetwork);
                    }
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

        private string GetLocalPath(IPAddress ip, Socket socket)
        {
            IPEndPoint localEndPoint;
            IPAddress remoteIp = ip;
            IPEndPoint remoteEndPoint = new IPEndPoint(remoteIp, 0);
            try
            {
                localEndPoint = QueryRoutingInterface(socket, remoteEndPoint);
            }
            catch(Exception)
            {
                if (IsIPv6)
                    throw new NotSupportedException("A error occured while trying to determine the local end point, the network or provider most likely does not support IPv6.");
                else
                    throw new InvalidOperationException("A error occured while trying to determine the local end point.");
            }
            return localEndPoint.Address.ToString();
        }

        private  IPEndPoint QueryRoutingInterface(
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

        public bool IsIPv6 => IPv6Mode || request.Ip.AddressFamily == AddressFamily.InterNetworkV6;
    }
}
