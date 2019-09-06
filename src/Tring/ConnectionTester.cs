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

namespace Tring
{
    internal class ConnectionTester
    {
        private readonly TimeSpan waitTime = TimeSpan.FromSeconds(1);

        public enum ConnectionStatus { Succes, TimeOut, Refused, Untried };
        private ConnectionRequest _request;

        public ConnectionTester(ConnectionRequest request)
        {
            _request = request;
        }

        public ConnectionResult TryConnect()
        {
            ConnectionStatus Connection, DNS;
            DNS = ConnectionStatus.Untried;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result;
            if (_request.Ip == null)
            {
                DNS = DnsLookup(_request.Url, out var ip);
                _request = new ConnectionRequest(ip, _request.Port, _request.Url);
                if (DNS != ConnectionStatus.Succes)
                    return new ConnectionResult(_request, DNS);
            }
            var watch = System.Diagnostics.Stopwatch.StartNew();
            result = socket.BeginConnect(_request.Ip, _request.Port, null, null);
            bool connectionSuccess = result.AsyncWaitHandle.WaitOne(waitTime);
            var localInterface = GetLocalPath(_request.Ip, socket);
            if (socket.Connected)
            {
                watch.Stop();
                socket.EndConnect(result);
                var connectionTimeMs = watch.ElapsedMilliseconds;
                Connection = ConnectionStatus.Succes;
                return new ConnectionResult(_request, DNS, Connection, ConnectionStatus.Untried, localInterface, connectionTimeMs);
            }
            else
            {
                socket.Close();
                var (Ping, PingTimeMs) = PingHost(_request.Ip);
                if (connectionSuccess)
                    Connection = ConnectionStatus.Refused;
                else
                    Connection = ConnectionStatus.TimeOut;
                return new ConnectionResult(_request, DNS, Connection, Ping, localInterface, 0, PingTimeMs);
            }
        }

        public static (ConnectionStatus status, long timeInMs) PingHost(IPAddress ip)
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send(ip);
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
                    ip = Dns.EndGetHostEntry(lookupResult)?.AddressList.FirstOrDefault(foundIp => foundIp.AddressFamily == AddressFamily.InterNetwork);
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

        private static string GetLocalPath(IPAddress ip, Socket socket)
        {
            IPAddress remoteIp = ip;
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
