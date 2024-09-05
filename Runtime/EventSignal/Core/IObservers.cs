namespace AceLand.EventDriven.EventSignal.Core
{
    public interface IObservers
    {
        void Trigger();
    }
    
    public interface IObservers<T>
    {
        void Trigger(in T value);
    }
}