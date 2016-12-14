using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Threading;
using log4net;
using Lunyx.Common.UI.Wpf.Collections;

namespace CasualMeter.Core.Helpers
{
    public class CollectionHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Lazy<CollectionHelper> Lazy = new Lazy<CollectionHelper>(() => new CollectionHelper());
        public static CollectionHelper Instance => Lazy.Value;
        private Dispatcher _uiDispatcher;

        private CollectionHelper() { }

        public void Initialize(Dispatcher uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;
        }

        public SyncedCollection<T> CreateSyncedCollection<T>(IEnumerable<T> enumerable = null)
        {
            var t =
                _uiDispatcher.InvokeAsync(
                    () => enumerable == null ? new SyncedCollection<T>() : new SyncedCollection<T>(enumerable),
                    DispatcherPriority.Send);
            return t.Result;
        }
    }
}
