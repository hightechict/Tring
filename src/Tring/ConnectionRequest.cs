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
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Tring
{
    class ConnectionRequest
    {
        private static readonly Regex SplitFormat = new Regex(@"^(?<host>.*):(?<port>\w+)$");

        public ConnectionRequest(IPAddress ip = null, int port = PortLogic.UnsetPort, string url = "", bool ipv6Mode = false)
        {
            Ip = ip;
            Url = url;
            Port = port;
            _ipv6Mode = ipv6Mode;
        }

        public static ConnectionRequest Parse(string input,bool ipv6Mode =false)
        {
            ConnectionRequest request;
            var match = SplitFormat.Match(input);
            var uriCreated = Uri.TryCreate(input, UriKind.Absolute, out var uri);
            if (uriCreated && !string.IsNullOrEmpty(uri.DnsSafeHost))
            {
                request = new ConnectionRequest(null, uri.Port, uri.DnsSafeHost, ipv6Mode);
            }
            else if (match.Success)
            {
                var host = match.Groups["host"].Value;
                var port = match.Groups["port"].Value;
                var convertedPort = PortLogic.StringToPort(port);
                if (convertedPort == PortLogic.UnsetPort)
                    convertedPort = PortLogic.DeterminePortByProtocol(port);
                if (!IPAddress.TryParse(host, out var ip))
                    request = new ConnectionRequest(null, convertedPort, host, ipv6Mode);
                else
                    request = new ConnectionRequest(ip, convertedPort,"" , ipv6Mode);

                if (request.Port == PortLogic.UnsetPort || request.Port < 0 || request.Port > ushort.MaxValue) throw new ArgumentException($"The input you provided for the port is not valid, your input: {request.Port}.");
            }
            else
            {
                throw new ArgumentException($"Invalid input: {input} is nether a valid url nor a host:port or host:protocol.");
            }
            return request;
        }

        public IPAddress Ip { get; }
        public string Url { get; }
        public int Port { get; }
        public bool IsIPv6 => Ip?.AddressFamily == AddressFamily.InterNetworkV6 || _ipv6Mode && Ip == null;
        private readonly bool _ipv6Mode;
    }
}
