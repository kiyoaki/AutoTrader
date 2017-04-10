using BitFlyer.Apis;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTrader
{
    public class Trader
    {
        private const double MinOrderSize = 0.001;
        private static readonly TimeSpan LoopSpan = TimeSpan.FromSeconds(10);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PrivateApi _privateApi;
        private readonly double _betting;
        private readonly TickerAnalyzer _analyzer;

        private Position _position;
        private double _positionSize;
        private Side? _orderSide;
        private double _orderSize;
        private DateTime? _positionStartTime;
        private Ticker _latestTicker;
        private DateTime _lastReceivedTime = DateTime.UtcNow;
        private double _positionPrice;


        public Trader(CommandLineOptions options)
        {
            _privateApi = new PrivateApi(options.ApiKey, options.ApiSecret);
            _betting = options.Betting;
            _analyzer = new TickerAnalyzer();
        }

        public Task Start(CancellationToken cancellationToken)
        {
            new RealtimeApi().Subscribe<Ticker>(PubnubChannel.TickerFxBtcJpy,
                ticker =>
                {
                    _lastReceivedTime = DateTime.UtcNow;
                    _latestTicker = ticker;
                },
                message => { Logger.Info(message); },
                (message, ex) =>
                {
                    Logger.Info(message);
                    if (ex != null)
                        Logger.Info(ex);
                });

            return Task.Factory.StartNew(CoreLoop, cancellationToken, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private async Task CoreLoop()
        {
            while (true)
            {
                var now = DateTime.UtcNow;

                Logger.Info("Last Receive: " + _lastReceivedTime);

                if (now - _lastReceivedTime < TimeSpan.FromSeconds(3) && _latestTicker != null)
                {
                    try
                    {
                        var positions = await _privateApi.GetPositions(ProductCode.FxBtcJpy);
                        var orders = await _privateApi.GetChildOrders(ProductCode.FxBtcJpy, 3);
                        orders = orders.Where(x => x.ChildOrderState == ChildOrderState.Active).ToArray();
                        _positionSize = 0;

                        var before = _position;

                        _position = Position.None;
                        if (positions.Any())
                        {
                            var list = new List<PriceAndSize>();
                            foreach (var position in positions)
                            {
                                _position = position.Side == Side.Buy ? Position.Long : Position.Short;
                                _positionSize += position.Size;

                                list.Add(new PriceAndSize { Price = position.Price, Size = position.Size });
                            }

                            _positionPrice = list.Sum(x => x.Price * x.Size / _positionSize);
                        }

                        if (_positionSize < MinOrderSize)
                            _position = Position.None;

                        var after = _position;

                        if (before != after)
                            _positionStartTime = now;
                        else if (_position != Position.None && _positionStartTime != null)
                            Logger.Info("Position TimeSpan: " + (now - _positionStartTime.Value));

                        _orderSide = null;
                        _orderSize = 0;
                        if (orders.Any())
                            foreach (var order in orders)
                            {
                                _orderSide = order.Side;
                                _orderSize += order.Size;
                            }

                        var result = _analyzer.Analyze(_position, _positionPrice, _latestTicker, now);
                        Logger.Info("Analyze Result: " + result);
                        var canBet = _betting - _positionSize;
                        switch (result)
                        {
                            case AnalysisResult.Hold:
                                break;

                            case AnalysisResult.Buy:
                                {
                                    var size = _latestTicker.BestAskSize > canBet
                                        ? canBet
                                        : _latestTicker.BestAskSize;

                                    if (size < MinOrderSize)
                                        break;

                                    switch (_orderSide)
                                    {
                                        case Side.Sell:
                                        case Side.BuySell:
                                            await _privateApi.CancelAllOrders(new CancelAllOrdersParameter
                                            {
                                                ProductCode = ProductCode.FxBtcJpy
                                            });
                                            Thread.Sleep(1000);
                                            break;
                                    }

                                    await _privateApi.SendChildOrder(new SendChildOrderParameter
                                    {
                                        Size = size,
                                        Side = Side.Buy,
                                        Price = (int)_latestTicker.BestAsk,
                                        ChildOrderType = ChildOrderType.Limit,
                                        ProductCode = ProductCode.FxBtcJpy,
                                        TimeInForce = TimeInForce.ImmediateOrCancel
                                    });
                                }
                                break;
                            case AnalysisResult.Sell:
                                {
                                    var size = _latestTicker.BestBidSize > canBet
                                        ? canBet
                                        : _latestTicker.BestBidSize;

                                    if (size < MinOrderSize)
                                        break;

                                    switch (_orderSide)
                                    {
                                        case Side.Buy:
                                        case Side.BuySell:
                                            await _privateApi.CancelAllOrders(new CancelAllOrdersParameter
                                            {
                                                ProductCode = ProductCode.FxBtcJpy
                                            });
                                            Thread.Sleep(1000);
                                            break;
                                    }

                                    await _privateApi.SendChildOrder(new SendChildOrderParameter
                                    {
                                        Size = size,
                                        Side = Side.Sell,
                                        Price = (int)_latestTicker.BestBid,
                                        ChildOrderType = ChildOrderType.Limit,
                                        ProductCode = ProductCode.FxBtcJpy,
                                        TimeInForce = TimeInForce.ImmediateOrCancel
                                    });
                                }
                                break;
                            case AnalysisResult.ProfitBuy:
                                {
                                    await _privateApi.SendChildOrder(new SendChildOrderParameter
                                    {
                                        Size = Math.Min(_positionSize, _latestTicker.BestAskSize),
                                        Side = Side.Buy,
                                        Price = (int)_latestTicker.BestAsk,
                                        ChildOrderType = ChildOrderType.Limit,
                                        ProductCode = ProductCode.FxBtcJpy,
                                        TimeInForce = TimeInForce.ImmediateOrCancel
                                    });
                                }
                                break;
                            case AnalysisResult.ProfitSell:
                                {
                                    await _privateApi.SendChildOrder(new SendChildOrderParameter
                                    {
                                        Size = Math.Min(_positionSize, _latestTicker.BestBidSize),
                                        Side = Side.Sell,
                                        Price = (int)_latestTicker.BestBid,
                                        ChildOrderType = ChildOrderType.Limit,
                                        ProductCode = ProductCode.FxBtcJpy,
                                        TimeInForce = TimeInForce.ImmediateOrCancel
                                    });
                                }
                                break;
                            case AnalysisResult.LossCutBuy:
                                {
                                    await _privateApi.SendChildOrder(new SendChildOrderParameter
                                    {
                                        Size = _positionSize,
                                        Side = Side.Buy,
                                        ChildOrderType = ChildOrderType.Market,
                                        ProductCode = ProductCode.FxBtcJpy,
                                        TimeInForce = TimeInForce.GoodTilCanceled
                                    });
                                }
                                break;
                            case AnalysisResult.LossCutSell:
                                {
                                    await _privateApi.SendChildOrder(new SendChildOrderParameter
                                    {
                                        Size = _positionSize,
                                        Side = Side.Sell,
                                        ChildOrderType = ChildOrderType.Market,
                                        ProductCode = ProductCode.FxBtcJpy,
                                        TimeInForce = TimeInForce.GoodTilCanceled
                                    });
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, ex.Message);
                        if (ex.InnerException != null)
                        {
                            Logger.Error(ex.InnerException, "InnerException: " + ex.InnerException.Message);
                            if (ex.InnerException.InnerException != null)
                            {
                                Logger.Error(ex.InnerException.InnerException,
                                    "InnerException: " + ex.InnerException.InnerException.Message);
                            }
                        }
                    }

                    Logger.Info("position: " + _position);
                    Logger.Info("positionSize: " + _positionSize);

                    Logger.Info("orderSide: " + _orderSide);
                    Logger.Info("orderSize: " + _orderSize);
                }

                Logger.Info("------------------------------");

                Thread.Sleep(LoopSpan);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private struct PriceAndSize
        {
            public double Price { get; set; }
            public double Size { get; set; }
        }
    }
}