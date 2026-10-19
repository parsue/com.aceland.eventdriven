#if UNITY_EDITOR
namespace AceLand.EventDriven.Core
{
    /// <summary>
    /// Editor-only configuration bridge. Compiled only under <c>UNITY_EDITOR</c> and
    /// therefore never ships in a player build.
    ///
    /// Rationale: <see cref="EventDrivenUtils.SignalGetterTimeout"/> has an
    /// <c>internal set</c> and the runtime assembly keeps zero dependency with no
    /// <c>InternalsVisibleTo</c> to the editor assembly. To let the editor Settings
    /// Provider push a configured value into the runtime, we expose this minimal
    /// public surface that lives inside the runtime assembly (so it can reach the
    /// internal setter) but is gated behind <c>UNITY_EDITOR</c> (so it is invisible
    /// to player builds and does not widen the shipped public API).
    /// </summary>
    public static class EventDrivenEditorConfig
    {
        /// <summary>Default signal-getter timeout (seconds) used when nothing is configured.</summary>
        public static float DefaultSignalGetterTimeout => EventDrivenUtils.DefaultSignalGetterTimeout;

        /// <summary>Current runtime signal-getter timeout (seconds).</summary>
        public static float SignalGetterTimeout => EventDrivenUtils.SignalGetterTimeout;

        /// <summary>
        /// Overwrite the runtime signal-getter timeout (seconds). Non-positive values
        /// fall back to <see cref="DefaultSignalGetterTimeout"/>.
        /// </summary>
        public static void SetSignalGetterTimeout(float seconds)
        {
            EventDrivenUtils.SignalGetterTimeout =
                seconds > 0f ? seconds : EventDrivenUtils.DefaultSignalGetterTimeout;
        }
    }
}
#endif
