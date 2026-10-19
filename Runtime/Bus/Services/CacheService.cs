using System;
using System.Collections.Generic;
using AceLand.EventDriven.Bus.Core;
using UnityEngine.SceneManagement;

namespace AceLand.EventDriven.Bus.Services
{
    internal sealed class CacheService
    {
        public static CacheService Build() => new();

        private CacheService()
        {
            _lock = new object();
            _eventCache = new Dictionary<Type, EventCache>();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private readonly object _lock;
        private readonly Dictionary<Type, EventCache> _eventCache;

        public void ClearEventCache(Type eventType)
        {
            bool removed;
            lock (_lock) removed = _eventCache.Remove(eventType);

#if UNITY_EDITOR
            if (removed)
                Diagnostics.EventMonitorBridge.NotifyClear(eventType, Diagnostics.EventClearCause.ExplicitCaller);
#endif
        }

        public void ClearAllEventCache()
        {
#if UNITY_EDITOR
            Type[] cleared;
            lock (_lock)
            {
                cleared = _eventCache.Count > 0 ? new Type[_eventCache.Count] : Array.Empty<Type>();
                _eventCache.Keys.CopyTo(cleared, 0);
                _eventCache.Clear();
            }

            foreach (var type in cleared)
                Diagnostics.EventMonitorBridge.NotifyClear(type, Diagnostics.EventClearCause.ExplicitCaller);
#else
            lock (_lock) _eventCache.Clear();
#endif
        }

        public void SetCache(Type eventType, EventCache cache)
        {
            lock (_lock) _eventCache[eventType] = cache;
        }

        public bool TryGetCache(Type eventType, out EventCache cache)
        {
            bool staleDropped;
            lock (_lock)
            {
                if (_eventCache.TryGetValue(eventType, out cache))
                {
                    if (cache.IsValid()) return true;

                    // Binding target is gone: treat as no cache and drop the stale entry.
                    _eventCache.Remove(eventType);
                    staleDropped = true;
                }
                else
                {
                    staleDropped = false;
                }

                cache = default;
            }

#if UNITY_EDITOR
            if (staleDropped)
                Diagnostics.EventMonitorBridge.NotifyClear(eventType, Diagnostics.EventClearCause.GameObjectDestroyed);
#endif
            return false;
        }

        public void WithCache(Type eventType, Action<EventCache> actionIfExists)
        {
            EventCache cache;
            bool staleDropped = false;
            lock (_lock)
            {
                if (!_eventCache.TryGetValue(eventType, out cache))
                    return;

                if (!cache.IsValid())
                {
                    _eventCache.Remove(eventType);
                    staleDropped = true;
                }
            }

            if (staleDropped)
            {
#if UNITY_EDITOR
                Diagnostics.EventMonitorBridge.NotifyClear(eventType, Diagnostics.EventClearCause.GameObjectDestroyed);
#endif
                return;
            }

            // Invoke outside the lock to avoid holding it during user callbacks.
            actionIfExists?.Invoke(cache);
        }

        private void OnSceneUnloaded(Scene scene)
        {
#if UNITY_EDITOR
            List<(Type type, Diagnostics.EventClearCause cause)> cleared = null;
#endif
            lock (_lock)
            {
                List<Type> toRemove = null;
                foreach (var kvp in _eventCache)
                {
                    var cache = kvp.Value;
                    var boundToScene = cache.IsBoundToScene(scene);
                    if (boundToScene || !cache.IsValid())
                    {
                        toRemove ??= new List<Type>();
                        toRemove.Add(kvp.Key);
#if UNITY_EDITOR
                        cleared ??= new List<(Type, Diagnostics.EventClearCause)>();
                        cleared.Add((kvp.Key, boundToScene
                            ? Diagnostics.EventClearCause.SceneLoaded
                            : Diagnostics.EventClearCause.GameObjectDestroyed));
#endif
                    }
                }

                if (toRemove == null) return;
                foreach (var type in toRemove)
                    _eventCache.Remove(type);
            }

#if UNITY_EDITOR
            if (cleared != null)
                foreach (var (type, cause) in cleared)
                    Diagnostics.EventMonitorBridge.NotifyClear(type, cause);
#endif
        }

        internal void InternalClearAll()
        {
            ClearAllEventCache();
        }

#if UNITY_EDITOR
        // Editor-only: current cache state for the monitor inspector.
        internal Diagnostics.EventCacheInfo EditorGetCacheInfo(Type eventType)
        {
            lock (_lock)
            {
                if (!_eventCache.TryGetValue(eventType, out var cache) || !cache.IsValid())
                    return new Diagnostics.EventCacheInfo(false, CacheBinding.Persistent, "null");

                var payload = cache.EventData;
                var payloadString = Diagnostics.EventPayloadFormatter.Format(payload);
                return new Diagnostics.EventCacheInfo(true, cache.Binding, payloadString);
            }
        }
#endif

        internal void InternalDispose()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }
}
