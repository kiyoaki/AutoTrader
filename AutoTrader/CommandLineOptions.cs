using CommandLine;
using CommandLine.Text;

namespace AutoTrader
{
    public class CommandLineOptions
    {
        [Option('b', "betting", Required = false, DefaultValue = 1.0, HelpText = "Betting BTC amount for orders.")]
        public double Betting { get; set; }

        [Option('k', "key", Required = true, HelpText = "bitFlyer API Key.")]
        public string ApiKey { get; set; }

        [Option('s', "secret", Required = true, HelpText = "bitFlyer API Secret.")]
        public string ApiSecret { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}