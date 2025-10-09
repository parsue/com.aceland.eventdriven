using System;
using System.Collections.Generic;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public class ReadonlySignal<T> : IReadonlySignal<T>,
        IComparable<Signal<T>>, IComparable<ReadonlySignal<T>>, IComparable<T>,
        IEquatable<Signal<T>>, IEquatable<ReadonlySignal<T>>, IEquatable<T>
    {
        internal ReadonlySignal(Signal<T> refSignal) =>
            _refSignal = refSignal;

        public string Id => _refSignal.Id;
        public T Value => _refSignal.Value ?? default;

        private readonly Signal<T> _refSignal;

        public void AddListener(Action<T> listener) =>
            _refSignal.AddListener(listener);

        public void RemoveListener(Action<T> listener) =>
            _refSignal.RemoveListener(listener);
        
        public void Trigger() => _refSignal.Trigger();

        public override string ToString() => Value.ToString();
        
        public bool Equals(T other) => 
            Comparer<T>.Default.Compare(Value, other) == 0;

        public bool Equals(Signal<T> other) => 
            other != null && Comparer<T>.Default.Compare(Value, other.Value) == 0;

        public bool Equals(ReadonlySignal<T> other) => 
            other != null && Comparer<T>.Default.Compare(Value, other.Value) == 0;

        public int CompareTo(Signal<T> other) =>
            other == null ? 1 : Comparer<T>.Default.Compare(Value, other.Value);

        public int CompareTo(ReadonlySignal<T> other) =>
            other == null ? 1 : Comparer<T>.Default.Compare(Value, other.Value);

        public int CompareTo(T other) =>
            other == null ? 1 : Comparer<T>.Default.Compare(Value, other);

        public static implicit operator T(ReadonlySignal<T> signal) => signal.Value;
    }
}
