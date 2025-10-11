namespace AceLand.EventDriven.EventSignal
{
    public interface IEventSignalRef
    {
        internal ISignal RefSignal { get; }
    }
    
    public interface IEventSignalRef<T>
    {
        internal ISignal<T> RefSignal { get; }
    }
}