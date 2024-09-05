namespace AceLand.EventDriven.EventSignal.Core
{
    public interface ISignal
    {
        string Id { get; }
        void Dispose();
    }
}