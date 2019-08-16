using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Tring
{
    class Program
    {
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

                var startTime = DateTime.Now;
                var result = connectionTester.TryConnect();
                var newResult = result;
                OutputPrinter.HideCursor();
                OutputPrinter.PrintTable();
                while (true)
                { 
                    OutputPrinter.ResetPrintLine();
                    OutputPrinter.PrintLogEntry(startTime,newResult);
                    if (optionWatch.Value() != "on")
                        break;

                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    newResult = connectionTester.TryConnect();
                    if (!result.SameOutcome(newResult))
                    {
                        result = newResult;
                        startTime = DateTime.Now;
                        Console.CursorTop++;
                    }
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
