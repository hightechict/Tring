namespace Tring
{
    class ConnectionRequest
    {
        public ConnectionRequest(string ip, ushort port, string url = "")
        {
            Ip = ip;
            Url = url;
            Port = port;
        }

        public string Ip { get; set; }
        public string Url { get; set; }
        public ushort Port { get; set; }
    }
}
