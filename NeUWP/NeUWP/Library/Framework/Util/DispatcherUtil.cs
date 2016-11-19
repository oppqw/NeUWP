using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace NeUWP.Utilities
{
    public static class DispatcherUtil
    {
        public static CoreDispatcher Dispatcher { get; private set; }

        private static object DispatcherLocker = new object();

        public static void SetDispatcher(CoreDispatcher dispatcher)
        {
            lock (DispatcherLocker)
            {
                if (dispatcher != null)
                {
                    Dispatcher = dispatcher;
                }
            }
        }

        public async static void Run(DispatchedHandler agileCallback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                await dispatcher.RunAsync(priority, agileCallback);
            }
        }

        public async static Task RunAsync(DispatchedHandler agileCallback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                await dispatcher.RunAsync(priority, agileCallback);
            }
        }

        public async static void Run(CoreDispatcher dispatcher, DispatchedHandler agileCallback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if (dispatcher != null)
            {
                await dispatcher.RunAsync(priority, agileCallback);
            }
            else
            {
                Run(agileCallback, priority);
            }
        }

        public async static Task RunAsync(CoreDispatcher dispatcher, DispatchedHandler agileCallback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if (dispatcher != null)
            {
                await dispatcher.RunAsync(priority, agileCallback);
            }
            else
            {
                await RunAsync(agileCallback, priority);
            }
        }
    }
}
