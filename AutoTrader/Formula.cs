using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoTrader
{
    public static class Formula
    {
        public static double LastRSI(this IEnumerable<double> source, int period)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var s = source as double[] ?? source.ToArray();
            var count = s.Length;

            if (count <= period) throw new ArgumentException(nameof(source) + ".Length <= period");

            var totalProfit = 0d;
            var totalLoss = 0d;

            for (var i = count - period; i < count; i++)
            {
                var profitOrLoss = s[i] - s[i - 1];
                if (profitOrLoss > 0)
                    totalProfit += profitOrLoss;
                else if (profitOrLoss < 0)
                    totalLoss += profitOrLoss;
            }

            return 100d * totalProfit / (totalProfit - totalLoss);
        }

        public static IEnumerable<double> RSI(this IEnumerable<double> source, int period)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var s = source as double[] ?? source.ToArray();
            var count = s.Length;

            if (count <= period) throw new ArgumentException(nameof(source) + ".Length <= period");

            IEnumerable<double> Func()
            {
                for (var offset = 0; offset < count - period; offset++)
                {
                    var targets = s.Skip(offset).Take(period + 1).ToArray();
                    yield return LastRSI(targets, period);
                }
            }

            return Func();
        }

        public static double LastEMA(this IEnumerable<double> source, int period)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var alpha = 2d / (1d + period);
            var result = 0d;
            var first = true;

            foreach (var price in source)
            {
                if (first)
                {
                    first = false;
                    result = price;
                    continue;
                }

                result = price * alpha + result * (1 - alpha);
            }

            return result;
        }

        public static IEnumerable<double> EMA(this IEnumerable<double> source, int period)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            IEnumerable<double> Func()
            {
                var alpha = 2d / (1d + period);
                var result = 0d;
                var first = true;

                foreach (var price in source)
                {
                    if (first)
                    {
                        first = false;
                        result = price;
                        continue;
                    }

                    yield return price * alpha + result * (1 - alpha);
                }
            }

            return Func();
        }

        public static MACD LastMACD(this IEnumerable<double> source, int period, int signalPeriod)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var macdSlow = 0d;
            var macd = 0d;
            var macdSignal = 0d;
            var first = true;
            var alpha = 2d / (1d + period);
            var signalAlpha = 2d / (1d + signalPeriod);

            foreach (var price in source)
            {
                if (first)
                {
                    first = false;
                    macdSlow = price;
                    macdSignal = 0d;
                    continue;
                }

                macdSlow = price * alpha + macdSlow * (1 - alpha);
                macd = price - macdSlow;
                macdSignal = macd * signalAlpha + macdSignal * (1 - signalAlpha);
            }

            return new MACD { Slow = macdSlow, Value = macd, Signal = macdSignal };
        }

        public static IEnumerable<MACD> MACD(this IEnumerable<double> source, int period, int signalPeriod)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            IEnumerable<MACD> Func()
            {
                var macdSlow = 0d;
                var macdSignal = 0d;
                var first = true;
                var alpha = 2d / (1d + period);
                var signalAlpha = 2d / (1d + signalPeriod);

                foreach (var price in source)
                {
                    if (first)
                    {
                        first = false;
                        macdSlow = price;
                        macdSignal = 0d;
                        continue;
                    }

                    macdSlow = price * alpha + macdSlow * (1 - alpha);
                    var macd = price - macdSlow;
                    macdSignal = macd * signalAlpha + macdSignal * (1 - signalAlpha);

                    yield return new MACD
                    {
                        Slow = macdSlow,
                        Value = macd,
                        Signal = macdSignal
                    };
                }
            }

            return Func();
        }
    }

    public struct MACD
    {
        public double Slow { get; set; }
        public double Value { get; set; }
        public double Signal { get; set; }
    }
}