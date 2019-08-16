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

namespace Tring
{
    internal class PortLogic
    {
        public const ushort UnsetPort = 0;
        public static ushort StringToPort(string toConvert)
        {
            ushort toReturn;
            if (!ushort.TryParse(toConvert, out toReturn))
            {
                toReturn = DeteminePortByProtocol(toConvert);
            }
            return toReturn;
        }
        public static ushort DeteminePortByProtocol(string protocol)
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
                case "rdp":
                    return 3389;
                default:
                    return 0;
            }
        }
    }
}
