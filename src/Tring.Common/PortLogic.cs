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

using System.Collections.Generic;
using System.Linq;

namespace Tring
{
    internal class PortLogic
    {
        public const int UnsetPort = -1;

        public static readonly Dictionary<string, int> PortByProtocols = new Dictionary<string, int>
        {
            ["ftp"] = 21,
            ["ssh"] = 22,
            ["smtp"] = 25,
            ["dns"] = 53,
            ["http"] = 80,
            ["pop"] = 110,
            ["imap"] = 143,
            ["snmp"] = 161,
            ["bgp"] = 179,
            ["ldap"] = 389,
            ["https"] = 443,
            ["smb"] = 445,
            ["smb2"] = 445,
            ["ldaps"] = 636,
            ["sql"] = 1433,
            ["rdp"] = 3389
        };

        public static int StringToPort(string toConvert)
        {
            if (!int.TryParse(toConvert, out var toReturn))
            {
                return DeterminePortByProtocol(toConvert);
            }
            return toReturn;
        }

        public static int DeterminePortByProtocol(string protocol)
        {
            if (PortByProtocols.ContainsKey(protocol))
                return PortByProtocols[protocol];
            else
                return UnsetPort;
        }

        public static string DetermineProtocolByPort(int port) => PortByProtocols.FirstOrDefault(pair => pair.Value == port).Key ?? "";
    }
}
