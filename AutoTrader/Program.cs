using System;
using System.Threading;
using AutoTrader.Extensions;
using CommandLine;

namespace AutoTrader
{
    internal class Program
    {
        private static readonly CancellationToken CancellationToken = new CancellationToken();

        private static void Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (Parser.Default.ParseArguments(args, options))
            {
                new Trader(options).Start(CancellationToken).FireAndForget();
            }

            Console.ReadKey();
        }
    }
}