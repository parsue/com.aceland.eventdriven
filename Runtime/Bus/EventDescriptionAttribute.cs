using System;

namespace AceLand.EventDriven.Bus
{
    /// <summary>
    /// Attaches a human-readable description to an event interface
    /// (<see cref="IEvent"/> / <see cref="IEvent{T}"/>). The description is read
    /// at compile time by the EventDriven Source Generator and baked into the
    /// generated registration — no runtime reflection is performed. It is surfaced
    /// by the Event Monitor window in both edit and play mode.
    /// </summary>
    /// <remarks>
    /// Apply to the event interface declaration:
    /// <code>
    /// [EventDescription("Raised when the player's health reaches zero.")]
    /// public interface IPlayerDiedEvent : IEvent { void OnPlayerDied(); }
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class EventDescriptionAttribute : Attribute
    {
        public EventDescriptionAttribute(string description)
        {
            Description = description ?? string.Empty;
        }

        /// <summary>Human-readable description of the event's purpose.</summary>
        public string Description { get; }
    }
}
