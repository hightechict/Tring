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
    class Program
    {
        static void Main(string[] args)
        {
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
                        throw new ArgumentException("No arguments provided: please provide a host and a port or protocol");
                    case 1:
                        connectionTester = new ConnectionTester(arguments.Values[0]);
                        break;
                    case 2:
                        connectionTester = new ConnectionTester(arguments.Values[0], arguments.Values[1]);
                        break;
                    default:
                        throw new ArgumentException("To many arguments provided: please provide only a host and a port");
                }
                OutputPrinter.SetCleanUp();
                OutputPrinter.PrintTable();
                var startTime = DateTime.Now;
                ConnectionResult result = new ConnectionResult();
                while (true)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var newResult = connectionTester.TryConnect();
                    if (result.SameOutcome(newResult))
                    {
                        if (!Console.IsOutputRedirected)
                            Console.CursorTop--;
                    }
                    else
                    {
                        result = newResult;
                        startTime = DateTime.Now;
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
                return 0;
            });
            var response = app.Execute(args);
        }
    }
}
