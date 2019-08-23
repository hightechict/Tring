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

using System.Net;

namespace Tring
{
    class ConnectionRequest
    {
        public ConnectionRequest(IPAddress ip = null, int port = PortLogic.UnsetPort, string url = "")
        {
            Ip = ip;
            Url = url;
            Port = port;
        }

        public IPAddress Ip { get; }
        public string Url { get; }
        public int Port { get; }
    }
}
