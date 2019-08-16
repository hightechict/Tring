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
        private static readonly Regex checkIfUrl = new Regex(@"^((?<protocol>http|https|ftp)\:\/\/)*(?<host>[\w\.\-~]+(\.[\w\.\-~]+)*)(\:(?<port>.+))?(\/.+)*", RegexOptions.Compiled);
        private readonly TimeSpan waitTime = TimeSpan.FromSeconds(1);

        public enum ConnectionStatus { Succes, TimeOut, Refused, Untried };

        public ConnectionRequest request;

        public ConnectionTester(string UrlOrIp, string port = "")
        {
            request = CheckIfIP(UrlOrIp);
            if (request == null)
                request = CheckIfURL(UrlOrIp);
            if (request == null)
                throw new ArgumentException($"Input: {UrlOrIp} is nether a ip nor a url.");

            if (!string.IsNullOrEmpty(port))
            {
                request.Port = PortLogic.StringToPort(port);
            }
            if (request.Port == PortLogic.UnsetPort) throw new ArgumentException($"The input you provided for the port is not valid, your input: {port}");
        }

        public ConnectionResult TryConnect()
        {
            var toReturn = new ConnectionResult();
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result;
            toReturn.Request = request;
            if (string.IsNullOrEmpty(request.Ip))
            {
                toReturn.DnsResult = DnsLookup(request.Url);
                if (toReturn.DnsResult != ConnectionStatus.Succes)
                    return toReturn;
            }
            result = socket.BeginConnect(request.Ip, request.Port, null, null);
            bool connectionSuccess = result.AsyncWaitHandle.WaitOne(waitTime);
            toReturn.LocalInterface = GetLocalPath(request.Ip, socket);
            if (socket.Connected)
            {
                socket.EndConnect(result);
                toReturn.Connect = ConnectionStatus.Succes;
            }
            else
            {
                socket.Close();
                toReturn.PingResult = PingHost(request.Ip);
                if (connectionSuccess)
                    toReturn.Connect = ConnectionStatus.Refused;
                else
                    toReturn.Connect = ConnectionStatus.TimeOut;
            }
            return toReturn;
        }
        public static ConnectionStatus PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable ? ConnectionStatus.Succes : ConnectionStatus.TimeOut;
        }

        private ConnectionStatus DnsLookup(string host)
        {
            IAsyncResult lookupResult = Dns.BeginGetHostEntry(host, null, null);
            bool DnsLookupSucces = lookupResult.AsyncWaitHandle.WaitOne(waitTime);
            if (DnsLookupSucces)
            {
                try
                {
                    request.Ip = Dns.EndGetHostEntry(lookupResult)?.AddressList[0]?.ToString();
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

        private static ConnectionRequest CheckIfIP(string toCheck)
        {
            var host = "";
            var port = PortLogic.UnsetPort;
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
            }
            else
            {
                return null;
            }
            return new ConnectionRequest(host, port);
        }

        private static ConnectionRequest CheckIfURL(string toCheck)
        {
            var url = "";
            var port = PortLogic.UnsetPort;
            var urlMatch = checkIfUrl.Match(toCheck);
            if (urlMatch.Success)
            {
                url = urlMatch.Groups["host"].Value;
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
            }
            else
            {
                return null;
            }
            return new ConnectionRequest("", port, url);
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
