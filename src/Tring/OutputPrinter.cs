using System;

namespace Tring
{
    internal class OutputPrinter
    {
        public static void PrintLogEntry(ConnectionTester.ConnectionStatus status, string host, ushort port, string currentTime, string startTime = "", string localTrace = "")
        {
            PrintTime(currentTime, startTime);
            PrintHost(host, port);
            PrintStatus(status);
            PrintEndPoint(localTrace);
        }
        public static void ResetPrintLine()
        {
            Console.SetCursorPosition(0,Console.CursorTop);
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
        private static void PrintStatus(ConnectionTester.ConnectionStatus status)
        {
            switch(status)
            {
                case ConnectionTester.ConnectionStatus.Succes:
                    PrintConnectionSucces();
                    break;
                case ConnectionTester.ConnectionStatus.DnsFailed:
                    PrintConnectionFailer("Dns lookup failed");
                    break;
                case ConnectionTester.ConnectionStatus.DnsTimeOut:
                    PrintConnectionFailer("Dns lookup timed out");
                    break;
                default:
                    PrintConnectionFailer(status.ToString());
                    break;
            }
        }
        private static void PrintTime(string currentTime,string startTime ="")
        {
            Console.ResetColor();
            if (string.IsNullOrEmpty(startTime))
            {
                Console.Write($"{currentTime} ");
            }
            else
            {
                Console.Write($"{startTime} - {currentTime} ");
            }
        }
        private static void PrintHost(string host, ushort port)
        {
            Console.ResetColor();
            Console.Write($"Connecting to {host}:{port}");
        }
        private static void PrintEndPoint(string endPointIp)
        {
            Console.ResetColor();
            if (!string.IsNullOrEmpty(endPointIp))
                Console.Write($" (local interface used: {endPointIp})");
        }
        private static void PrintConnectionSucces()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" OK");
            Console.ResetColor();
        }
        private static void PrintConnectionFailer(string reason)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" NOK : {reason}");
            Console.ResetColor();
        }
    }
}
