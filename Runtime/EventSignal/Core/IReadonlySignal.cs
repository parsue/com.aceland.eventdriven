namespace AceLand.EventDriven.EventSignal.Core
{
    public interface IReadonlySignal<out T>
    {
        string Id { get; }
        T Value { get; }
    }
}