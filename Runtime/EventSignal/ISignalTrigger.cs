using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public interface ISignalTrigger : IEventSignal
    {
        void Trigger();
    }
}