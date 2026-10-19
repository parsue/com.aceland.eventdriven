namespace AceLand.EventDriven.Bus.Core
{
    /// <summary>
    /// Defines the lifetime of an event cache entry.
    /// </summary>
    public enum CacheBinding
    {
        /// <summary>
        /// Default. The cache is kept until it is manually cleared
        /// (<c>EventBus.ClearCache</c> / <c>EventBus.ClearAllCache</c>) or the application quits.
        /// No lifecycle binding is applied.
        /// </summary>
        Persistent = 0,

        /// <summary>
        /// The cache is bound to a <see cref="UnityEngine.GameObject"/>. When the bound GameObject is
        /// destroyed the cache is treated as if it no longer exists.
        /// </summary>
        GameObject = 1,

        /// <summary>
        /// The cache is bound to the Scene it was created in. When that Scene is unloaded the cache is
        /// treated as if it no longer exists.
        /// </summary>
        Scene = 2,
    }
}
