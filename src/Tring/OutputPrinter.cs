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

namespace Tring
{
    internal class OutputPrinter
    {
        private const string timeFormat = "HH:mm:ss";

        public static void PrintLogEntry(DateTime startTime, ConnectionResult status)
        {
            PrintTime(startTime, status.TimeStamp);
            PrintRequest(status);
            PrintResult(status.Connect, status.ConnectionTimeMs);
            PrintPing(status.PingResult, status.PingTimeMs);
            PrintLocalInterface(status.LocalInterface);
        }

        public static void PrintTable()
        {
            Console.WriteLine("| Time              | IP              | Port  | Connect | Ping    | Local Interface |");
            //example output   | 20:22:22-20:23:33 | 100.10.23.44    | 80222 | Timeout | 1000 ms | 111.111.111.111 |
        }

        public static void ResetPrintLine()
        {
            if (!Console.IsOutputRedirected)
                Console.SetCursorPosition(0, Console.CursorTop);
        }

        public static void HideCursor()
        {
            if (!Console.IsOutputRedirected)
                Console.CursorVisible = false;
        }

        public static void CleanUp()
        {
            if (!Console.IsOutputRedirected)
            {
                Console.CursorVisible = true;
                Console.ResetColor();
                Console.CursorTop++;
            }
        }
        public static void SetCleanUp()
        {
            Console.CancelKeyPress += delegate {
                if (!Console.IsOutputRedirected)
                {
                    CleanUp();
                }
            };
        }

        private static void PrintLocalInterface(string localInterface)
        {
            Console.ResetColor();
            Console.Write($"{localInterface.PadRight(15)} |\n");
        }

        private static void PrintPing(ConnectionTester.ConnectionStatus pingResult, long responseTimeMS)
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
            Console.Write($"{result.PadRight(7)}");
            Console.ResetColor();
            Console.Write(" | ");
        }

        private static void PrintResult(ConnectionTester.ConnectionStatus connect, long responseTimeMS)
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
            Console.Write($"{result.PadRight(7)}");
            Console.ResetColor();
            Console.Write(" | ");
        }

        private static void PrintRequest(ConnectionResult request)
        {
            var ipOrError = "";
            switch (request.DnsResult)
            {
                case ConnectionTester.ConnectionStatus.Succes:
                case ConnectionTester.ConnectionStatus.Untried:
                    ipOrError = request.Request.Ip;
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
            Console.Write($"{ipOrError.PadRight(15)}");
            Console.ResetColor();
            Console.Write($" | {request.Request.Port.ToString().PadRight(5)} | ");
        }

        private static void PrintTime(DateTime startTime, DateTime currentTime)
        {
            var toPrint = $"{startTime.ToString(timeFormat)}";
            if (startTime.ToString(timeFormat) != currentTime.ToString(timeFormat))
            {
                toPrint += $"-{currentTime.ToString(timeFormat)}";
            }
            Console.ResetColor();
            Console.Write($"| {toPrint.PadRight(17)} | ");
        }
    }
}
