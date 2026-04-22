namespace AceLand.EventDriven.Bus.Core
{
    public struct EventCache
    {
        public readonly object EventData;

        public EventCache(object eventData)
        {
            EventData = eventData;
        }
    }
}