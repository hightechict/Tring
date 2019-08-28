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
using Microsoft.Extensions.CommandLineUtils;

namespace Tring
{
    public class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "Tring"
            };
            app.HelpOption("-?|-h|--help");
            var arguments = app.Argument("arguments", "Enter the ip or url you wish to test.");
            var optionWatch = app.Option("-w|--watch", "Set the application to continually check the connection at the specified interval in seconds.", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                ConnectionTester connectionTester;
                switch (arguments.Values.Count)
                {
                    case 0:
                        throw new ArgumentException("No arguments provided: please provide only a host:port or host:protocol.");
                    case 1:
                        connectionTester = new ConnectionTester(arguments.Values[0]);
                        break;
                    default:
                        throw new ArgumentException("To many arguments provided: please provide only a host:port or host:protocol.");
                }
                Connect(optionWatch, connectionTester);
                return 0;
            });
            try
            {
                return app.Execute(args);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(e.Message);
                Console.ResetColor();
                return -1;
            }
        }

        private static void Connect(CommandOption optionWatch, ConnectionTester connectionTester)
        {
            OutputPrinter.SetupCleanUp();
            OutputPrinter.PrintTable();
            var startTime = DateTimeOffset.Now;
            ConnectionResult result = null;
            while (true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var newResult = connectionTester.TryConnect();
                if (result?.IsEquivalent(newResult) ?? false)
                {
                    if (!Console.IsOutputRedirected)
                        Console.CursorTop--;
                }
                else
                {
                    result = newResult;
                    startTime = newResult.TimeStamp;
                }
                OutputPrinter.ResetPrintLine();
                OutputPrinter.PrintLogEntry(startTime, newResult);
                if (optionWatch.Value() != "on")
                    break;
                OutputPrinter.HideCursor();

                if (watch.ElapsedMilliseconds < 1000)
                {
                    System.Threading.Thread.Sleep(1000 - (int)watch.ElapsedMilliseconds);
                }
            }
            OutputPrinter.CleanUp();
        }
    }
}
