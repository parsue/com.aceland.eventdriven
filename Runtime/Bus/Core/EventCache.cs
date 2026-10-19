using UnityEngine;
using UnityEngine.SceneManagement;

namespace AceLand.EventDriven.Bus.Core
{
    public readonly struct EventCache
    {
        public readonly object EventData;
        public readonly CacheBinding Binding;

        // GameObject binding target (null unless Binding == GameObject).
        private readonly GameObject _boundGameObject;

        // Scene binding target (default unless Binding == Scene).
        private readonly Scene _boundScene;

        public EventCache(object eventData)
            : this(eventData, CacheBinding.Persistent, null, default)
        {
        }

        private EventCache(object eventData, CacheBinding binding, GameObject boundGameObject, Scene boundScene)
        {
            EventData = eventData;
            Binding = binding;
            _boundGameObject = boundGameObject;
            _boundScene = boundScene;
        }

        public static EventCache Persistent(object eventData) =>
            new(eventData, CacheBinding.Persistent, null, default);

        public static EventCache BoundToGameObject(object eventData, GameObject target) =>
            new(eventData, CacheBinding.GameObject, target, default);

        public static EventCache BoundToScene(object eventData, Scene scene) =>
            new(eventData, CacheBinding.Scene, null, scene);

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
