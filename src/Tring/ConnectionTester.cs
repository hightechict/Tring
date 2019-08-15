using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Tring
{
    internal class ConnectionTester
    {
        private static readonly Regex checkIfIp = new Regex(@"^(?<ip>\d+\.\d+\.\d+\.\d+)(\:(?<port>.+))?$", RegexOptions.Compiled);
        private static readonly Regex checkIfUrl = new Regex(@"^((?<protocol>http|https|ftp)\:\/\/)*(?<host>[\w\.\-~]+(\.[\w\.\-~]+)*)(\:(?<port>.+))?(\/.+)*", RegexOptions.Compiled);
        private readonly bool HostIsURL;
        private readonly TimeSpan waitTime = TimeSpan.FromSeconds(1);

        public enum ConnectionStatus { Succes, TimeOut, DnsFailed, DnsTimeOut, Refused };
        public string Host { get; }
        public ushort Port { get; }

        public ConnectionTester(string UrlOrIp, string port = "")
        {
            if (CheckIfIP(UrlOrIp, out var hostFound, out var portFound))
            {
                Port = portFound;
                Host = hostFound;
                HostIsURL = false;
            }
            else if (CheckIfURL(UrlOrIp, out hostFound, out portFound))
            {
                Port = portFound;
                Host = hostFound;
                HostIsURL = true;
            }
            else
            {
                throw new ArgumentException($"Input: {UrlOrIp} is nether a ip nor a url.");
            }

            if (!string.IsNullOrEmpty(port))
            {
                Port = PortLogic.StringToPort(port);
            }
            if (Port == PortLogic.UnsetPort) throw new ArgumentException($"The input you provided for the port is not valid, your input: {port}");
        }

        public ConnectionStatus TryConnect(out string connectionPath)
        {
            string ip;
            connectionPath = "";
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result;
            if (HostIsURL)
            {
                var lookupResult = DnsLookup(Host, waitTime, out var host);
                if (host == null) return lookupResult;
                ip = host.AddressList[0].ToString();
                result = socket.BeginConnect(ip, Port, null, null);
            }
            else
            {
                ip = Host;
                result = socket.BeginConnect(ip, Port, null, null);
            }
            bool connectionSuccess = result.AsyncWaitHandle.WaitOne(waitTime);
            if (socket.Connected)
            {
                socket.EndConnect(result);
                return ConnectionStatus.Succes;
            }
            else
            {
                connectionPath = GetLocalPath(ip, socket);
                socket.Close();
                if (connectionSuccess)
                {
                    return ConnectionStatus.Refused;
                }
                else
                {
                    return ConnectionStatus.TimeOut;
                }
            }
        }

        private static ConnectionStatus DnsLookup(string host, TimeSpan timeToTry, out IPHostEntry result)
        {
            result = null;
            IAsyncResult lookupResult = Dns.BeginGetHostEntry(host, null, null);
            bool DnsLookupSucces = lookupResult.AsyncWaitHandle.WaitOne(timeToTry);
            if (DnsLookupSucces)
            {
                try
                {
                    result = Dns.EndGetHostEntry(lookupResult);
                    return ConnectionStatus.Succes;
                }
                catch (Exception)
                {
                    return ConnectionStatus.DnsFailed;
                }
            }
            else
            {
                return ConnectionStatus.DnsTimeOut;
            }
        }

        private static bool CheckIfIP(string toCheck, out string host, out ushort port)
        {
            host = "";
            port = PortLogic.UnsetPort;
            var ipMatch = checkIfIp.Match(toCheck);
            if (ipMatch.Success)
            {
                host = ipMatch.Groups["ip"].Value;
                if (!string.IsNullOrEmpty(ipMatch.Groups["port"].Value))
                {
                    port = PortLogic.StringToPort(ipMatch.Groups["port"].Value);
                    if (port == PortLogic.UnsetPort)
                    {
                        port = PortLogic.DeteminePortByProtocol(ipMatch.Groups["port"].Value);
                    }
                }
                return true;
            }
            return false;
        }

        private static bool CheckIfURL(string toCheck, out string host, out ushort port)
        {
            host = "";
            port = PortLogic.UnsetPort;
            var urlMatch = checkIfUrl.Match(toCheck);
            if (urlMatch.Success)
            {
                host = urlMatch.Groups["host"].Value;
                if (!string.IsNullOrEmpty(urlMatch.Groups["port"].Value))
                {
                    port = PortLogic.StringToPort(urlMatch.Groups["port"].Value);
                    if (port == PortLogic.UnsetPort)
                    {
                        port = PortLogic.DeteminePortByProtocol(urlMatch.Groups["port"].Value);
                    }
                }
                else if (port == PortLogic.UnsetPort && !string.IsNullOrEmpty(urlMatch.Groups["protocol"].Value))
                {
                    port = PortLogic.DeteminePortByProtocol(urlMatch.Groups["protocol"].Value);
                }
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
