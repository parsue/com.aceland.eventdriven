namespace AceLand.EventDriven.EventSignal
{
    public interface IReadonlySignal<out T> : ISignalListener<T>
    {
        T Value { get; }
        string ToString();
    }
}