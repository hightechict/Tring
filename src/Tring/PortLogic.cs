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
                toReturn = DeterminePortByProtocol(toConvert);
            }
            return toReturn;
        }
        public static ushort DeterminePortByProtocol(string protocol)
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
                case "smb":
                case "smb2":
                    return 445;
                case "ldaps":
                    return 636;
                case "sql":
                    return 1433;
                case "rdp":
                    return 3389;
                default:
                    return 0;
            }
        }
        public static string DetermineProtocolByPort(ushort port)
        {
            switch (port)
            {
                case 21:
                    return "ftp";
                case 22:
                    return "ssh";
                case 25:
                    return "smtp";
                case 53:
                    return "dns";
                case 80:
                    return "http";
                case 110:
                    return "pop";
                case 143:
                    return "imap";
                case 161:
                    return "snmp";
                case 179:
                    return "bgp";
                case 389:
                    return "ldap";
                case 443:
                    return "https";
                case 445:
                    return "smb2";
                case 636:
                    return "ldaps";
                case 1433:
                    return "sql";
                case 3389:
                    return "rdp";
                default:
                    return "Unknown";
            }
        }
    }
}
