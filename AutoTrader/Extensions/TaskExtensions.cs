using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTrader.Extensions
{
    public static class TaskExtensions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void FireAndForget(this Task task, string additionalMessage = null)
        {
            task.ConfigureAwait(false);
            task.ContinueWith(x =>
            {
                if (x.Exception != null)
                    logger.Error(x.Exception, $"TaskUnhandled {additionalMessage}: {x.Exception.Message}");
                else
                    logger.Error("TaskUnhandled {0} ", additionalMessage);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void FireAndForget(this Task task, Action<Exception> exceptionCallback)
        {
            task.ConfigureAwait(false);
            task.ContinueWith(x => { exceptionCallback?.Invoke(x.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static Task WhenAll(this IEnumerable<Task> tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return Task.WhenAll(tasks);
        }
    }
}