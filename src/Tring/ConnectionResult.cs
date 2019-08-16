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
