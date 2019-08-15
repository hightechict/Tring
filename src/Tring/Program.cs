using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Tring
{
    class Program
    {
        private const string timeFormat = "HH:mm:ss";
        static void Main(string[] args)
        {
            if (args.Length < 1) return;

            var app = new CommandLineApplication
            {
                Name = "quick-connect"
            };
            app.HelpOption("-?|-h|--help");
            var arguments = app.Argument("arguments", "Enter the ip or url you wish to test.", multipleValues: true);
            var optionWatch = app.Option("-w|--watch", "Set the application to continually check the connection at the specified interval in seconds.", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                ConnectionTester connectionTester;
                switch (arguments.Values.Count)
                {
                    case 0:
                        throw new ArgumentException("No arguments provided: please provide atleast a host");
                    case 1:
                        connectionTester = new ConnectionTester(arguments.Values[0]);
                        break;
                    case 2:
                        connectionTester = new ConnectionTester(arguments.Values[0], arguments.Values[1]);
                        break;
                    default:
                        throw new ArgumentException("To many arguments provided: please provide only a host and a port");
                }

                var startTime = DateTime.Now.ToString(timeFormat);
                var status = connectionTester.TryConnect(out var localTrace);
                OutputPrinter.HideCursor();
                while (true)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var newStatus = connectionTester.TryConnect(out localTrace);
                    if (status != newStatus)
                    {
                        status = newStatus;
                        Console.CursorTop++;
                    }
                    OutputPrinter.ResetPrintLine();
                    OutputPrinter.PrintLogEntry(
                                        status,
                                        connectionTester.Host, connectionTester.Port,
                                        DateTime.Now.ToString(timeFormat), startTime, localTrace);

                    if (optionWatch.Value() != "on")
                        break;
                    watch.Stop();
                    if (watch.ElapsedMilliseconds < 1000)
                    {
                        System.Threading.Thread.Sleep(1000- (int)watch.ElapsedMilliseconds);
                    } 
                }
                OutputPrinter.CleanUp();
                return 0;
            });
            var response = app.Execute(args);
        }
    }
}
