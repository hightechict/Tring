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
            PrintResult(status.Connect);
            PrintPing(status.PingResult);
            PrintLocalInterface(status.LocalInterface);
        }

        public static void PrintTable()
        {
            Console.WriteLine("| Time              | IP              | Port  | Result  | Ping | LocalInterface  |");
            //example output      | 20:22:22-20:23:33 | 100.10.23.44    | 80222 | Timeout | ✓    | 111.111.111.111 |
        }

        public static void ResetPrintLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        public static void HideCursor()
        {
            Console.CursorVisible = false;
        }

        public static void CleanUp()
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }

        private static void PrintLocalInterface(string localInterface)
        {
            Console.ResetColor();
            Console.Write($"{localInterface.PadRight(15)} |");
        }

        private static void PrintPing(ConnectionTester.ConnectionStatus pingResult)
        {
            var result = "";
            switch (pingResult)
            {
                case ConnectionTester.ConnectionStatus.Succes:
                    Console.ForegroundColor = ConsoleColor.Green;
                    result = "PONG";
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
            Console.Write($"{result.PadRight(4)}");
            Console.ResetColor();
            Console.Write(" | ");
        }

        private static void PrintResult(ConnectionTester.ConnectionStatus connect)
        {
            var result = "";
            switch (connect)
            {
                case ConnectionTester.ConnectionStatus.Succes:
                    Console.ForegroundColor = ConsoleColor.Green;
                    result = "OK";
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
            var toPrint = $"{startTime.ToString(timeFormat)}-{currentTime.ToString(timeFormat)}";
            Console.ResetColor();
            Console.Write($"| {toPrint.PadRight(17)} | ");
        }
    }
}
