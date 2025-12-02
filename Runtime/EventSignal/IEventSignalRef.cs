namespace AceLand.EventDriven.EventSignal
{
    public interface IEventSignalRef
    {
        internal ISignal RefSignal { get; }
        bool Disposed { get; }
    }
    
    public interface IEventSignalRef<T>
    {
        internal ISignal<T> RefSignal { get; }
        bool Disposed { get; }
    }
}