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
using static Tring.Common.ConnectionTester;

namespace Tring.Common
{
    class ConnectionResult
    {
        public ConnectionResult(
            ConnectionRequest request,
            ConnectionStatus dnsResult = ConnectionStatus.Untried,
            ConnectionStatus connect = ConnectionStatus.Untried,
            ConnectionStatus ping = ConnectionStatus.Untried,
            string localInterface = "",
            long connectionTime = 0,
            long pingTime = 0
            )
        {
            TimeStamp = DateTimeOffset.Now;
            DnsResult = dnsResult;
            Connect = connect;
            PingResult = ping;
            Request = request;
            LocalInterface = localInterface;
            ConnectionTimeMs = connectionTime;
            PingTimeMs = pingTime;
        }

        public ConnectionRequest Request { get; }
        public DateTimeOffset TimeStamp { get; }
        public ConnectionStatus DnsResult { get; }
        public ConnectionStatus Connect { get; }
        public long ConnectionTimeMs { get; }
        public ConnectionStatus PingResult { get; }
        public long PingTimeMs { get; }
        public string LocalInterface { get; }

        public bool IsEquivalent(ConnectionResult result)
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
