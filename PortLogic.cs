namespace QuickConnect
{
    internal class PortLogic
    {
        public const short UnsetPort = 0;
        public static short StringToPort(string toConvert)
        {
            short toReturn;
            if (!short.TryParse(toConvert, out toReturn))
            {
                toReturn = DeteminePortByProtocol(toConvert);
            }
            return toReturn;
        }
        public static short DeteminePortByProtocol(string protocol)
        {
            switch (protocol.ToLower())
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
