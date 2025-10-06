using UnityEngine;

namespace AceLand.EventDriven.Profiles
{
    public abstract class SignalProviderBase : ScriptableObject
    {
        public virtual void PrewarmSignal() { }
        public virtual void Dispose() { }
    }
}