using CommandLine;

namespace AutoTrader
{
    public class CommandLineOptions
    {
        [Option('b', "betting", Required = false, Default = 1.0, HelpText = "Betting BTC amount for orders.")]
        public double Betting { get; set; }

        [Option('k', "key", Required = true, HelpText = "bitFlyer API Key.")]
        public string ApiKey { get; set; }

        [Option('s', "secret", Required = true, HelpText = "bitFlyer API Secret.")]
        public string ApiSecret { get; set; }
    }
}