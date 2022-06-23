using AutoTrader.Extensions;
using CommandLine;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Threading;

namespace AutoTrader
{
    internal class Program
    {
        private static readonly CancellationToken CancellationToken = new CancellationToken();

        private static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            NLog.LogManager.Configuration = config;

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(options =>
                {
                    new Trader(options).Start(CancellationToken).FireAndForget();
                });
            Console.ReadKey();
        }
    }
}