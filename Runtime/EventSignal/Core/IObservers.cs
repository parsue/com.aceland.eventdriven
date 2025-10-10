namespace AceLand.EventDriven.EventSignal.Core
{
    internal interface IObservers
    {
        void Trigger();
    }
    
    internal interface IObservers<T>
    {
        void Trigger(in T value);
    }
}