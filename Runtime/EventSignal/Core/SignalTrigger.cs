namespace AceLand.EventDriven.EventSignal.Core
{
    internal class SignalTrigger : ISignalTrigger
    {
        internal static SignalTrigger Build(ISignal refSignal) =>
            new(refSignal);
        private SignalTrigger(ISignal refSignal) =>
            _refSignal = refSignal;


        ISignal IEventSignalRef.RefSignal => _refSignal;
        private readonly ISignal _refSignal;

        public string Id => _refSignal.Id;
        
        public void Trigger() => 
            _refSignal.Trigger();
    }
    
    internal class SignalTrigger<T> : ISignalTrigger<T>
    {
        internal static SignalTrigger<T> Build(ISignal<T> refSignal) =>
            new SignalTrigger<T>(refSignal);
        
        private SignalTrigger(ISignal<T> refSignal) =>
            _refSignal = refSignal;

        ISignal<T> IEventSignalRef<T>.RefSignal => _refSignal;
        private readonly ISignal<T> _refSignal;

        public string Id => _refSignal.Id;
        
        public void Trigger() => 
            _refSignal.Trigger();
    }
}