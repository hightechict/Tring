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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tring
{
    internal class OutputPrinter
    {
        private const string timeFormat = "HH:mm:ss";
        private readonly object _lockObject;
        private readonly int _lenghtHostIP, _lenghtEgressIP, _lenghtPort;
        private const int _maximumLengthIPv4 = 15;
        private const int _maximumLengthIPv6 = 39;
        private const int _minimumLengthPort = 4;

        public OutputPrinter(IEnumerable<ConnectionRequest> ConnectionRequests)
        {
            _lockObject = new object();
            var lenghtPort = ConnectionRequests.Select(request => request.Port).Max(port => port.ToString().Length);
            _lenghtPort = lenghtPort >= _minimumLengthPort ? lenghtPort : _minimumLengthPort;
            _lenghtHostIP = ConnectionRequests.Select(request => request.Ip).Max(ip => ip?.ToString()?.Length) ?? 0;

            var hasURL = ConnectionRequests.Any(tester => !string.IsNullOrEmpty(tester.Url));

            if (hasURL && _lenghtHostIP < _maximumLengthIPv4)
                _lenghtHostIP = _maximumLengthIPv4;

            _lenghtEgressIP = _lenghtHostIP > _maximumLengthIPv4 ? _maximumLengthIPv6 : _maximumLengthIPv4;
        }

        public void PrintLogEntry(DateTimeOffset startTime, ConnectionResult status, int index)
        {
            lock (_lockObject)
            {
                SetPrintLine(index);
                PrintTime(startTime, status.TimeStamp);
                PrintRequest(status);
                PrintResult(status.Connect, status.ConnectionTimeMs);
                PrintPing(status.PingResult, status.PingTimeMs);
                PrintLocalInterface(status.LocalInterface);
                PrintProtocol(status.Request.Port);
                PrintHostName(status.Request.Url);
            }
        }

        public void PrintTable()
        {
            Console.WriteLine(" | Time              | " + "IP".PadRight(_lenghtHostIP) + " | " + "Port".PadRight(_lenghtPort) + " | Connect | Ping    | " + "Egress".PadRight(_lenghtEgressIP) + " | Protocol | Hostname");
            // example output   | 20:22:22-20:23:33 | 100.100.203.104 | 80222 | Timeout | 1000 ms | 111.111.111.111 | https    | google.com
            // IPv6             | 21:22:33-22:22:22 | 2001:4860:4860:1023:1230:1230:2330:8888 | 80222 | Timeout | 1000 ms | 2001:4860:4860:1023:1230:1230:2330:8888 | https    | google.com 
        }

        public static void SetPrintLine(int lines = 0)
        {
            if (!Console.IsOutputRedirected)
                Console.SetCursorPosition(0, lines);
        }

        public void HideCursor()
        {
            if (!Console.IsOutputRedirected)
                Console.CursorVisible = false;
        }

        public static void CleanUpConsole(int endLine)
        {
            if (!Console.IsOutputRedirected)
            {
                Console.CursorVisible = true;
                Console.ResetColor();
                if (Console.CursorTop <= endLine)
                    SetPrintLine(endLine + 1);
                else
                    Console.CursorTop++;
            }
        }

        public void SetCancelEventHandler(CancellationTokenSource sourceToken, int endLine)
        {
            Console.CancelKeyPress += (sender, args) =>
            {
                sourceToken.Cancel();
                CleanUpConsole(endLine);
            };
        }

        private void PrintProtocol(int port)
        {
            Console.Write($"{PortLogic.DetermineProtocolByPort(port),-8} | ");
        }

        private void PrintHostName(string url)
        {
            Console.ResetColor();
            Console.WriteLine(url);
        }

        private void PrintLocalInterface(string localInterface)
        {
            Console.ResetColor();
            Console.Write($"{localInterface.PadRight(_lenghtEgressIP)} | ");
        }

        private void PrintPing(ConnectionTester.ConnectionStatus pingResult, long responseTimeMS)
        {
            var result = "";
            switch (pingResult)
            {
                case ConnectionTester.ConnectionStatus.Succes:
                    Console.ForegroundColor = ConsoleColor.Green;
                    result = $"{responseTimeMS.ToString()} ms";
                    break;
                case ConnectionTester.ConnectionStatus.Untried:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    result = "-";
                    break;
                case ConnectionTester.ConnectionStatus.TimeOut:
                case ConnectionTester.ConnectionStatus.Refused:
                    Console.ForegroundColor = ConsoleColor.Red;
                    result = "NOK";
                    break;
            }
            Console.Write($"{result,-7}");
            Console.ResetColor();
            Console.Write(" | ");
        }

        private void PrintResult(ConnectionTester.ConnectionStatus connect, long responseTimeMS)
        {
            var result = "";
            switch (connect)
            {
                case ConnectionTester.ConnectionStatus.Succes:
                    Console.ForegroundColor = ConsoleColor.Green;
                    result = $"{responseTimeMS.ToString()} ms";
                    break;
                case ConnectionTester.ConnectionStatus.TimeOut:
                    result = "Timeout";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ConnectionTester.ConnectionStatus.Refused:
                    result = "Refused";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.Write($"{result,-7}");
            Console.ResetColor();
            Console.Write(" | ");
        }

        private void PrintRequest(ConnectionResult request)
        {
            var ipOrError = "";
            switch (request.DnsResult)
            {
                case ConnectionTester.ConnectionStatus.Succes:
                case ConnectionTester.ConnectionStatus.Untried:
                    ipOrError = request.Request.Ip.ToString();
                    break;
                case ConnectionTester.ConnectionStatus.TimeOut:
                    ipOrError = "DNS timeout";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ConnectionTester.ConnectionStatus.Refused:
                    ipOrError = "DNS not found";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.Write($"{ipOrError.PadRight(_lenghtHostIP)}");
            Console.ResetColor();
            Console.Write($" | {request.Request.Port.ToString().PadRight(_lenghtPort)} | ");
        }

        private void PrintTime(DateTimeOffset startTime, DateTimeOffset currentTime)
        {
            var toPrint = $"{startTime.ToString(timeFormat)}";
            if (startTime.ToString(timeFormat) != currentTime.ToString(timeFormat))
            {
                toPrint += $"-{currentTime.ToString(timeFormat)}";
            }
            Console.ResetColor();
            Console.Write($" | {toPrint,-17} | ");
        }
    }
}
