using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public static class SignalExtension
    {
        public static IReadonlySignal<T> AsReadonly<T>(this ISignal<T> signal) => new ReadonlySignal<T>(signal);
        public static ISignalListener AsListener(this ISignal signal) => signal;
        public static ISignalListener<T> AsListener<T>(this ISignal<T> signal) => signal;
        public static ISignalTrigger AsTrigger(this ISignal signal) => signal;
        public static ISignalTrigger AsTrigger<T>(this ISignal<T> signal) => signal;
        
    }
}