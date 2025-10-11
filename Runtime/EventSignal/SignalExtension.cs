using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public static class SignalExtension
    {
        // Signal to
        public static IReadonlySignal<T> AsReadonly<T>(this ISignal<T> signal) =>
            ReadonlySignal<T>.Build(signal);
        public static ISignalListener AsListener(this ISignal signal) => 
            SignalListener.Build(signal);
        public static ISignalListener<T> AsListener<T>(this ISignal<T> signal) =>
            SignalListener<T>.Build(signal);
        public static ISignalTrigger AsTrigger(this ISignal signal) =>
            SignalTrigger.Build(signal);
        public static ISignalTrigger<T> AsTrigger<T>(this ISignal<T> signal) =>
            SignalTrigger<T>.Build(signal);
        
        // Readonly Signal to
        public static ISignalListener<T> AsListener<T>(this IReadonlySignal<T> signal) =>
            SignalListener<T>.Build(signal.RefSignal);
        public static ISignalTrigger<T> AsTrigger<T>(this IReadonlySignal<T> signal) =>
            SignalTrigger<T>.Build(signal.RefSignal);
        
        // Signal Listener to
        public static IReadonlySignal<T> AsReadonly<T>(this ISignalListener<T> signal) =>
            ReadonlySignal<T>.Build(signal.RefSignal);
        public static ISignalTrigger AsTrigger(this ISignalListener signal) =>
            SignalTrigger.Build(signal.RefSignal);
        public static ISignalTrigger<T> AsTrigger<T>(this ISignalListener<T> signal) =>
            SignalTrigger<T>.Build(signal.RefSignal);
        
        // Signal Trigger to
        public static IReadonlySignal<T> AsReadonly<T>(this ISignalTrigger<T> signal) =>
            ReadonlySignal<T>.Build(signal.RefSignal);
        public static ISignalListener AsListener<T>(this ISignalTrigger signal) =>
            SignalListener.Build(signal.RefSignal);
        public static ISignalTrigger<T> AsTrigger<T>(this ISignalTrigger<T> signal) =>
            SignalTrigger<T>.Build(signal.RefSignal);
    }
}