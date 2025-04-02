namespace AceLand.EventDriven.Bus
{
    // for storage latest event data
    public struct EventCache
    {
        public readonly object Sender;
        public readonly object EventData;

        public EventCache(object sender, object eventData)
        {
            Sender = sender;
            EventData = eventData;
        }
    }
}