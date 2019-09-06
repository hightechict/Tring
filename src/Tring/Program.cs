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
            var arguments = app.Argument("arguments", "Enter the ip or url you wish to test.", true);
            var optionWatch = app.Option("-w|--watch", "Set the application to continually check the connection at the specified interval in seconds.", CommandOptionType.NoValue);

            app.OnExecute(async () =>
            {
                switch (arguments.Values.Count)
                {
                    case 0:
                        throw new ArgumentException("No arguments provided: please provide only a host:port or host:protocol.");
                    default:
                        await SetupConnections(optionWatch.Value() == "on", arguments.Values);
                        OutputPrinter.CleanUpConsole(arguments.Values.Count);
                        break;
                }
                return 0;
            });
            try
            {
                return app.Execute(args);
            }
            catch (AggregateException exception)
            {
                var errors = exception.InnerExceptions.Where(e => !(e is TaskCanceledException));
                foreach (var e in errors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.Message);
                    Console.ResetColor();
                }
                return errors.Any() ? -1 : 0;
            }
            catch (Exception exception) when (!(exception is TaskCanceledException))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(exception.Message);
                Console.ResetColor();
                return -1;
            }
        }

        private static Task SetupConnections(bool watchMode, List<string> connections)
        {
            var tasks = new List<Task>();
            var requests = connections.Select(connection => ConnectionRequest.Parse(connection));
            var connectors = requests.Select(r => new ConnectionTester(r)).ToList();
            var printer = new OutputPrinter(requests);
            var cancelationSource = new CancellationTokenSource();
            var cancelationToken = cancelationSource.Token;
            printer.SetCancelEventHandler(cancelationSource, Console.CursorTop + connectors.Count);
            if (watchMode)
                tasks.Add(CheckIfEscPress(cancelationSource));
            printer.PrintTable();
            printer.HideCursor();
            for (int index = 0; index < connectors.Count; index++)
            {
                tasks.Add(
                    Connect(
                        watchMode,
                        watchMode && connectors.Count == 1,
                        connectors[index],
                        Console.CursorTop + index,
                        printer,
                        cancelationToken
                    ));
            }
            return Task.WhenAll(tasks);
        }

        private static async Task Connect(bool watchMode, bool addLineOnChange, ConnectionTester connection, int index, OutputPrinter printer, CancellationToken cancelationToken)
        {
            await Task.Run(async () =>
            {
                ConnectionResult oldResult = null;
                while (!cancelationToken.IsCancellationRequested)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var result = connection.TryConnect();
                    if (oldResult == null)
                        oldResult = result;
                    if (!oldResult.IsEquivalent(result))
                    {
                        oldResult = result;
                        if (addLineOnChange)
                            index++;
                    }
                    printer.PrintLogEntry(oldResult.TimeStamp, result, index);
                    if (!watchMode)
                        break;

                    if (watch.ElapsedMilliseconds < 1000)
                    {
                        await Task.Delay(1000 - (int)watch.ElapsedMilliseconds, cancelationToken);
                    }

                }
            });
        }

        private static async Task CheckIfEscPress(CancellationTokenSource tokenSource)
        {
            await Task.Run(async () =>
            {
                do
                {
                    if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                    {
                        tokenSource.Cancel();
                    }
                    else
                    {
                        await Task.Delay(200, tokenSource.Token);
                    }
                } while (!tokenSource.Token.IsCancellationRequested);
            });
        }
    }
}
