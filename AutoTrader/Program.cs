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
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });
            loggerFactory.ConfigureNLog("nlog.config");

            var logger = loggerFactory.CreateLogger("default");

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(options =>
                {
                    new Trader(options).Start(CancellationToken).FireAndForget();
                });
            Console.ReadKey();
        }
    }
}