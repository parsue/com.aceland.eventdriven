namespace AceLand.EventDriven.EventSignal
{
    public static class SignalExtension
    {
        public static ReadonlySignal<T> AsReadonly<T>(this Signal<T> signal) => signal;
    }
}