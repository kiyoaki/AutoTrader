namespace AutoTrader
{
    public enum Position
    {
        None,
        Short,
        Long
    }

    public enum PriceTrend
    {
        None,
        Rise,
        GreatRise,
        Fall,
        GreatFall,
    }

    public enum AnalysisResult
    {
        Hold,
        Buy,
        Sell,
        ProfitBuy,
        ProfitSell,
        LossCutBuy,
        LossCutSell
    }
}