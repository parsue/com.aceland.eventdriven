using System;
#if ENABLE_PROFILER
using AceLand.EventDriven;
#endif

namespace AceLand.EventDriven.EventSignal.Core
{
    internal class Observers : IDisposable, IObservers
    {
#if ENABLE_PROFILER
        // Set by the owning signal/linker so the profiler scope can be named
        // after the signal Id. Built once; never per-trigger.
        internal string ProfilerScopeName { get; set; }
#endif
        internal Observers(params Action[] listeners)
        {
            foreach (var listener in listeners)
                Listeners += listener;
        }
        
        ~Observers() => Dispose();

        public virtual void Dispose()
        {
            Listeners = null;
        }
        
        private event Action Listeners;

        public void AddListener(Action listener) => Listeners += listener;
        public void RemoveListener(Action listener) => Listeners -= listener;
        public void Clear() => Listeners = null;

        public void Trigger()
        {
#if ENABLE_PROFILER
            if (Listeners is null) return;
            var prefix = ProfilerScopeName ?? "AceLand.EventDriven.Signal";
            using (EventProfiler.NamedScope(prefix).Auto())
            {
                var list = Listeners.GetInvocationList();
                for (var i = 0; i < list.Length; i++)
                {
                    var listener = (Action)list[i];
                    using (EventProfiler.Listener(prefix, listener).Auto())
                        listener.Invoke();
                }
            }
#else
            Listeners?.Invoke();
#endif
        }
    }
    
    internal class Observers<T> : IDisposable, IObservers<T>
    {
#if ENABLE_PROFILER
        internal string ProfilerScopeName { get; set; }
#endif

        internal Observers(params Action<T>[] listeners)
        {
            foreach (var listener in listeners)
                Listeners += listener;
        }
        
        ~Observers() => Dispose();

        public virtual void Dispose()
        {
            Listeners = null;
        }
        
        private event Action<T> Listeners;

        public void AddListener(Action<T> listener) => Listeners += listener;
        public void RemoveListener(Action<T> listener) => Listeners -= listener;
        public void Clear() => Listeners = null;

        public void Trigger(in T value)
        {
#if ENABLE_PROFILER
            if (Listeners is null) return;
            var prefix = ProfilerScopeName ?? "AceLand.EventDriven.Signal";
            using (EventProfiler.NamedScope(prefix).Auto())
            {
                var list = Listeners.GetInvocationList();
                for (var i = 0; i < list.Length; i++)
                {
                    var listener = (Action<T>)list[i];
                    using (EventProfiler.Listener(prefix, listener).Auto())
                        listener.Invoke(value);
                }
            }
#else
            Listeners?.Invoke(value);
#endif
        }
    }
}
