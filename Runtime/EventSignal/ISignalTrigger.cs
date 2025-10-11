using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public interface ISignalTrigger : IEventSignal, IEventSignalRef
    {
        void Trigger();
    }
    
    public interface ISignalTrigger<T> : IEventSignal, IEventSignalRef<T>
    {
        void Trigger();
    }
}