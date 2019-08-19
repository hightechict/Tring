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
using static Tring.ConnectionTester;

namespace Tring
{
    class ConnectionResult
    {
        public ConnectionResult()
        {
            TimeStamp = DateTime.Now;
            DnsResult = ConnectionStatus.Untried;
            Connect = ConnectionStatus.Untried;
            PingResult = ConnectionStatus.Untried;
            Request = null;
            LocalInterface = "";
        }

        public ConnectionRequest Request { get; set; }
        public DateTime TimeStamp { get; set; }
        public ConnectionStatus DnsResult { get; set; }
        public ConnectionStatus Connect { get; set; }
        public ConnectionStatus PingResult { get; set; }
        public long PingTimeMS { get; set; }
        public string LocalInterface { get; set; }

        public bool SameOutcome(ConnectionResult result)
        {
            return
                Request.Ip == result.Request.Ip &&
                Request.Port == result.Request.Port &&
                Connect == result.Connect &&
                PingResult == result.PingResult &&
                LocalInterface == LocalInterface;
        }
    }
}
