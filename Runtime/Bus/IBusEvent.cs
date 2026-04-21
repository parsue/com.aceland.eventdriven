namespace AceLand.EventDriven.Bus
{
    public interface IBusEvent { }
    public interface IEvent : IBusEvent { }
    public interface IEvent<TData> : IBusEvent { }
}