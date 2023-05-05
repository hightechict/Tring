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
using McMaster.Extensions.CommandLineUtils;

namespace Tring.Common
{
    public class Program
    {
        private static readonly int startLine = Console.CursorTop;
        private const int extraSpacing = 2;

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "Tring"
            };
            app.HelpOption("-?|-h|--help");
            var arguments = app.Argument("arguments", "Enter the ip or url you wish to test. Multiple host entered space separated.", true);
            var optionWatch = app.Option("-w|--watch", "Set the application to continually check the connection at the specified interval in seconds.", CommandOptionType.NoValue);
            var optionIPv6 = app.Option("-6|--ipv6", "Set the program to use IPv6 for dns requests.", CommandOptionType.NoValue);

            app.OnExecuteAsync(async _ =>
            {
                switch (arguments.Values.Count)
                {
                    case 0:
                        throw new ArgumentException("No arguments provided: please provide only a host:port or host:protocol.");
                    default:
                        var cancelationSource = new CancellationTokenSource();
                        var watch = optionWatch.HasValue();
                        var IPv6 = optionIPv6.HasValue();
                        await SetupConnections(optionWatch.HasValue(), optionIPv6.HasValue(), arguments.Values, cancelationSource);
                        FinalCleanup(cancelationSource, startLine + arguments.Values.Count, extraSpacing);
                        break;
                }
                return 0;
            });
            try
            {
                var exiCode = app.Execute(args);
                return exiCode;
            }
            catch (AggregateException exception)
            {
                foreach (var e in exception.InnerExceptions)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.Message);
                    Console.ResetColor();
                }
                return -1;
            }
            catch (Exception exception) when (!(exception is TaskCanceledException))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(exception.Message);
                Console.ResetColor();
                return -1;
            }
            finally
            {
                app.Dispose();
            }
        }

        private static Task SetupConnections(bool watchMode, bool IPv6Mode, IEnumerable<string> connections, CancellationTokenSource cancelationSource)
        {
            var tasks = new List<Task>();
            var requests = connections.Select(connection => ConnectionRequest.Parse(connection, IPv6Mode));
            var connectors = requests.Select(r => new ConnectionTester(r)).ToList();
            var printer = new OutputPrinter(requests);
            var cancelationToken = cancelationSource.Token;
            SetCancelEventHandler(cancelationSource, startLine + connectors.Count, extraSpacing);
            if (watchMode)
                tasks.Add(CheckIfEscPress(cancelationSource));
            printer.PrintTable();
            printer.HideCursor();
            if (!Console.IsOutputRedirected)
            {
                for (int index = 0; index < connectors.Count; index++)
                    Console.WriteLine("");
            }
            var cursorTop = Console.CursorTop - connectors.Count;
            for (int index = 0; index < connectors.Count; index++)
            {
                tasks.Add(
                    Connect(
                        watchMode,
                        watchMode && connectors.Count == 1,
                        connectors[index],
                        cursorTop + index,
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
                        try
                        {
                            await Task.Delay(1000 - (int)watch.ElapsedMilliseconds, cancelationToken);
                        }
                        catch (Exception) { }
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
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
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

        private static void SetCancelEventHandler(CancellationTokenSource sourceToken, int endLine, int spacing)
        {
            Console.CancelKeyPress += (sender, args) =>
            {
                FinalCleanup(sourceToken, endLine, spacing);
            };
        }

        private static void FinalCleanup(CancellationTokenSource sourceToken, int endLine, int spacing)
        {
            sourceToken.Cancel();
            OutputPrinter.CleanUpConsole(endLine, spacing);
            sourceToken.Dispose();
        }
    }
}
