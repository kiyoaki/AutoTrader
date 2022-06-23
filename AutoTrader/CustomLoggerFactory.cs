using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace AutoTrader
{
    public static class CustomLoggerFactory
    {
        public static ILogger<T> Create<T>()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });
            loggerFactory.ConfigureNLog("nlog.config");
            return loggerFactory.CreateLogger<T>();
        }

        public static ILogger Create(string categoryName)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });
            loggerFactory.ConfigureNLog("nlog.config");
            return loggerFactory.CreateLogger(categoryName);
        }
    }
}
