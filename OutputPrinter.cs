using System;

namespace QuickConnect
{
    public class OutputPrinter
    {
        private static ConsoleColor consoleColor = Console.ForegroundColor;
        public static void PrintConnectionSucces(string host,short port)
        {
            Console.Write($"Connecting to {host}:{port}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" OK\n");
            Console.ForegroundColor = consoleColor;
        }
        public static void PrintConnectionFailer(string host, short port,string reason)
        {
            Console.Write($"Connecting to {host}:{port}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" NOK : {reason}\n");
            Console.ForegroundColor = consoleColor;
        }
    }
}
