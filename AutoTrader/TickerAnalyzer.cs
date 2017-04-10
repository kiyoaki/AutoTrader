using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BitFlyer.Apis;
using MathNet.Numerics.Statistics;
using NLog;

namespace AutoTrader
{
    public class TickerAnalyzer
    {
        private const int MaxLength = 1024;
        private const int ResearchCount = 100;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<Ticker> _tickers = new ConcurrentQueue<Ticker>();
        private readonly ConcurrentQueue<double> _priceMovements = new ConcurrentQueue<double>();
        private readonly ConcurrentQueue<EMA> _emas = new ConcurrentQueue<EMA>();

        private double _priceMovementAverage;
        private double _priceMovementStandardDev;

        public AnalysisResult Analyze(Position position, double positionPrice, Ticker bitFlyer, DateTime utcNow)
        {
            if (bitFlyer == null)
                return AnalysisResult.Hold;

            Logger.Info("Price: " + bitFlyer.LatestPrice);
            Logger.Info("Volume: " + bitFlyer.Volume);

            EnqueueTicker(bitFlyer);

            var prices = _tickers.Select(x => x.LatestPrice).ToArray();
            var ema = new EMA
            {
                Ten = prices.LastEMA(10),
                Twenty = prices.LastEMA(20),
                Thirty = prices.LastEMA(30)
            };
            EnqueueEMA(ema);
            Logger.Info(ema.ToString);

            var priceMovement = GetPriceMovement(_emas.Select(x => x.Ten).ToArray(), 10);
            EnqueuePriceMovement(priceMovement);

            if (_tickers.Count < ResearchCount)
            {
                Logger.Info("##TickerLength##" + _tickers.Count);
                return AnalysisResult.Hold;
            }

            var priceTrend = GetPriceTrend(priceMovement);

            Logger.Info("Price Trend: " + priceTrend);

            switch (position)
            {
                case Position.None:
                    switch (priceTrend)
                    {
                        case PriceTrend.GreatRise:
                            return AnalysisResult.Buy;
                        case PriceTrend.GreatFall:
                            return AnalysisResult.Sell;
                    }
                    break;

                case Position.Short:
                    switch (priceTrend)
                    {
                        case PriceTrend.None:
                            if (bitFlyer.BestAsk < positionPrice)
                            {
                                return AnalysisResult.ProfitBuy;
                            }
                            break;
                        case PriceTrend.Rise:
                        case PriceTrend.GreatRise:
                            return AnalysisResult.LossCutBuy;
                        case PriceTrend.GreatFall:
                            return AnalysisResult.Sell;
                    }
                    break;
                case Position.Long:
                    switch (priceTrend)
                    {
                        case PriceTrend.None:
                            if (bitFlyer.BestBid > positionPrice)
                            {
                                return AnalysisResult.ProfitSell;
                            }
                            break;
                        case PriceTrend.GreatRise:
                            return AnalysisResult.Buy;
                        case PriceTrend.Fall:
                        case PriceTrend.GreatFall:
                            return AnalysisResult.LossCutSell;
                    }
                    break;
            }

            return AnalysisResult.Hold;
        }

        private static double GetPriceMovement(IReadOnlyCollection<double> prices, int lastOffset)
        {
            if (prices == null || prices.Count < 2)
                return 0.0;

            var offset = Math.Max(prices.Count - lastOffset, 0);
            return prices.Last() - prices.Skip(offset).First();
        }

        private void EnqueueTicker(Ticker item)
        {
            while (_tickers.Count >= MaxLength)
                DequeueTicker();

            _tickers.Enqueue(item);
        }

        private void DequeueTicker()
        {
            Ticker item;
            _tickers.TryDequeue(out item);
        }

        private void EnqueuePriceMovement(double item)
        {
            while (_priceMovements.Count >= MaxLength)
                DequeuePriceMovement();

            _priceMovements.Enqueue(item);
        }

        private void DequeuePriceMovement()
        {
            double item;
            _priceMovements.TryDequeue(out item);
        }

        private void EnqueueEMA(EMA item)
        {
            const int maxLength = 20;
            while (_emas.Count >= maxLength)
                DequeueEMA();

            _emas.Enqueue(item);
        }

        private void DequeueEMA()
        {
            EMA item;
            _emas.TryDequeue(out item);
        }

        private PriceTrend GetPriceTrend(double priceMovement)
        {
            _priceMovementAverage = _priceMovements.Average();
            _priceMovementStandardDev = _priceMovements.StandardDeviation();

            var separation = 0d;
            if (Math.Abs(_priceMovementStandardDev) > 0d)
                separation = (priceMovement - _priceMovementAverage) / _priceMovementStandardDev;

            Logger.Info("PriceMovement          : " + priceMovement);
            Logger.Info("PriceMovement Average  : " + _priceMovementAverage);
            Logger.Info("PriceMovement Statistic: " + separation);

            var priceMoveStatsAbs = Math.Abs(separation);

            if (priceMovement > 0)
            {
                if (priceMoveStatsAbs >= 3)
                {
                    return PriceTrend.GreatRise;
                }

                if (priceMoveStatsAbs >= 1)
                {
                    return PriceTrend.Rise;
                }
            }

            if (priceMovement < 0)
            {
                if (priceMoveStatsAbs >= 3)
                {
                    return PriceTrend.GreatFall;
                }
                if (priceMoveStatsAbs >= 1)
                {
                    return PriceTrend.Fall;
                }
            }

            return PriceTrend.None;
        }

        private struct EMA
        {
            public double Ten { get; set; }

            public double Twenty { private get; set; }

            public double Thirty { private get; set; }

            public override string ToString()
            {
                return $"EMA Ten:{Ten} Twenty:{Twenty} Thirty:{Thirty}";
            }
        }
    }
}