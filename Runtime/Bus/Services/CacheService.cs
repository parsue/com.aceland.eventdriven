using System;
using System.Collections.Generic;
using AceLand.EventDriven.Bus.Core;

namespace AceLand.EventDriven.Bus.Services
{
    internal sealed class CacheService
    {
        public static CacheService Build() => new();
        private CacheService() {}
        
        private readonly object _lock = new();
        private readonly Dictionary<Type, EventCache> _eventCache = new();

        public void ClearEventCache(Type eventType)
        {
            lock (_lock) _eventCache.Remove(eventType);
        }

        public void ClearAllEventCache()
        {
            lock (_lock) _eventCache.Clear();
        }

        public void SetCache(Type eventType, EventCache cache)
        {
            lock (_lock) _eventCache[eventType] = cache;
        }

        public bool TryGetCache(Type eventType, out EventCache cache)
        {
            lock (_lock)
            {
                return _eventCache.TryGetValue(eventType, out cache);
            }
        }

        public void WithCache(Type eventType, Action<EventCache> actionIfExists)
        {
            lock (_lock)
            {
                if (_eventCache.TryGetValue(eventType, out var cache))
                {
                    actionIfExists?.Invoke(cache);
                }
            }
        }

        internal void InternalClearAll()
        {
            ClearAllEventCache();
        }
    }
}