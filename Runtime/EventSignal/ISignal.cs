namespace AceLand.EventDriven.EventSignal
{
    public interface ISignal : ISignalListener
    {
        string ToString();
        void Dispose();
    }

    public interface ISignal<T> : ISignalListener<T>
    {
        T Value { get; set; }
        string ToString();
        void Dispose();
    }
}