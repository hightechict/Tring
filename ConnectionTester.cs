using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace QuickConnect
{
    class ConnectionTester
    {
        private static readonly Regex checkIfIp = new Regex(@"^(?<ip>\d+\.\d+\.\d+\.\d+)(?<port>\:\d+)*$", RegexOptions.Compiled);
        private static readonly Regex checkIfUrl = new Regex(@"^((?<protocol>http|https|ftp)\:\/\/)*(?<host>[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+)(\:(?<port>\d+))?(\/[a-zA-Z0-9]+)*", RegexOptions.Compiled);
        public string Host { get; }
        public short Port { get; }
        public ConnectionTester(string UrlOrIp, string port = "")
        {
            var urlMatch = checkIfUrl.Match(UrlOrIp);
            var ipMatch = checkIfIp.Match(UrlOrIp);
            if (ipMatch.Success)
            {
                Host = ipMatch.Groups["ip"].Value;
                if (!string.IsNullOrEmpty(ipMatch.Groups["port"].Value))
                {
                    port = ipMatch.Groups["port"].Value;
                }
            }
            else if (urlMatch.Success)
            {
                Host = urlMatch.Groups["host"].Value;
                if(!string.IsNullOrEmpty(urlMatch.Groups["port"].Value))
                {
                    port = urlMatch.Groups["port"].Value;
                }
                else if(string.IsNullOrEmpty(port) && !string.IsNullOrEmpty(urlMatch.Groups["protocol"].Value))
                {
                    Port = DeteminePortByProtocol(urlMatch.Groups["protocol"].Value);
                }
            }
            else
            {
               throw new ArgumentException($"Input: {UrlOrIp} is nether a ip nor url.");
            }

            if (!string.IsNullOrEmpty(port))
            {
                if (!short.TryParse(port, out var parsedPort))
                {
                    Port = DeteminePortByProtocol(port);
                    if(Port == 0) throw new ArgumentException($"The number you provided is not valid: {port}");
                }
                Port = parsedPort;
            }   
        }
        public void TryConnectAndPrintResult()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IAsyncResult result = socket.BeginConnect(Host, Port, null, null);

            result.AsyncWaitHandle.WaitOne(1000, true);

            if (socket.Connected)
            {
                socket.EndConnect(result);
                OutputPrinter.PrintConnectionSucces(Host,Port);
            }
            else
            {
                OutputPrinter.PrintConnectionFailer(Host, Port,"Not yet implemented :/");
                socket.Close();
            }
        }

        private short DeteminePortByProtocol(string protocol)
        {
            switch(protocol.ToLower())
            {
                case "ftp":
                    return 21;
                case "ssh":
                    return 22;
                case "smtp":
                    return 25;
                case "dns":
                    return 53;
                case "http":
                    return 80;
                case "pop":
                    return 110;
                case "imap":
                    return 143;
                case "snmp":
                    return 161;
                case "bgp":
                    return 179;
                case "ldap":
                    return 389;
                case "https":
                    return 443;
                case "ldaps":
                    return 636;
                default:
                    return 0;
            }
        }

    }
}
