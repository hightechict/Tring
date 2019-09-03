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
using System.Threading.Tasks;
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
            var arguments = app.Argument("arguments", "Enter the ip or url you wish to test.",true);
            var optionWatch = app.Option("-w|--watch", "Set the application to continually check the connection at the specified interval in seconds.", CommandOptionType.NoValue);

            app.OnExecute(async () =>
            {
                switch (arguments.Values.Count)
                {
                    case 0:
                        throw new ArgumentException("No arguments provided: please provide only a host:port or host:protocol.");
                    default:
                        await Connect(optionWatch.Value() == "on", arguments.Values);
                        break;
                }
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

        private static Task Connect(bool watchMode, List<string> connections)
        {
            var connectors = new List<ConnectionTester>();
            var currentResults = new ConnectionResult[connections.Count];
            var oldResults = new ConnectionResult[connections.Count];
            foreach (string input in connections)
            {
                connectors.Add(new ConnectionTester(input));
            }
            OutputPrinter.SetupCleanUp();
            OutputPrinter.PrintTable();
            while (true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Parallel.For(0, connectors.Count, async (i) =>
                {
                    currentResults[i] = await connectors[i].TryConnect();
                    if (oldResults[i] == null)
                    {
                        oldResults[i] = currentResults[i];
                    }
                    else if (!oldResults[i].IsEquivalent(currentResults[i]))
                    {
                        oldResults[i] = currentResults[i];
                        if(connections.Count == 1)
                        {
                            OutputPrinter.ResetPrintLine(-1);
                        }
                    }
                });
                OutputPrinter.PrintLogEntry(oldResults,currentResults);
                if (!watchMode)
                    break;
                OutputPrinter.HideCursor();
                if (ExitAplication)
                {
                    break;
                }
                OutputPrinter.ResetPrintLine(connectors.Count);
                if (watch.ElapsedMilliseconds < 1000)
                {
                    System.Threading.Thread.Sleep(1000 - (int)watch.ElapsedMilliseconds);
                }
            }
            OutputPrinter.CleanUp();
            return null;
        }

        private static bool ExitAplication => Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape;
    }
}
