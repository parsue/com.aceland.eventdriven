using UnityEngine;
using UnityEngine.SceneManagement;

namespace AceLand.EventDriven.Bus.Core
{
    public readonly struct EventCache
    {
        // Typed payload box (null for no-data events). Keeps the payload in a strongly
        // typed field so value types are never boxed on write/read. See ICachePayload.
        public readonly ICachePayload Payload;
        public readonly CacheBinding Binding;

        // GameObject binding target (null unless Binding == GameObject).
        private readonly GameObject _boundGameObject;

        // Scene binding target (default unless Binding == Scene).
        private readonly Scene _boundScene;

        public EventCache(ICachePayload payload)
            : this(payload, CacheBinding.Persistent, null, default)
        {
        }

        private EventCache(ICachePayload payload, CacheBinding binding, GameObject boundGameObject, Scene boundScene)
        {
            Payload = payload;
            Binding = binding;
            _boundGameObject = boundGameObject;
            _boundScene = boundScene;
        }

        public static EventCache Persistent(ICachePayload payload) =>
            new(payload, CacheBinding.Persistent, null, default);

        public static EventCache BoundToGameObject(ICachePayload payload, GameObject target) =>
            new(payload, CacheBinding.GameObject, target, default);

        public static EventCache BoundToScene(ICachePayload payload, Scene scene) =>
            new(payload, CacheBinding.Scene, null, scene);

        /// <summary>
        /// Lazily determines whether this cache entry is still valid against its binding target.
        /// Invalid entries should be treated as if no cache exists.
        /// </summary>
        public bool IsValid()
        {
            switch (Binding)
            {
                case CacheBinding.GameObject:
                    // Unity-overloaded null check: destroyed GameObject compares equal to null.
                    return _boundGameObject;

                case CacheBinding.Scene:
                    // An unloaded scene reports IsValid() == false (or isLoaded == false).
                    return _boundScene.IsValid() && _boundScene.isLoaded;

                case CacheBinding.Persistent:
                default:
                    return true;
            }
        }

        /// <summary>
        /// Returns true when this entry is bound to <paramref name="scene"/>.
        /// </summary>
        public bool IsBoundToScene(Scene scene) =>
            Binding == CacheBinding.Scene && _boundScene == scene;
    }
}
