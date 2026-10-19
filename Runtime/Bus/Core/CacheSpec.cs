using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AceLand.EventDriven.Bus.Core
{
    /// <summary>
    /// Describes how a raised event's cache should be recorded.
    /// Carried through the raiser builder chain; converted into an
    /// <see cref="EventCache"/> at raise time via <see cref="ToCache"/>.
    /// Absence of a CacheSpec (null) means "do not record a cache".
    /// </summary>
    internal readonly struct CacheSpec
    {
        private readonly CacheBinding _binding;
        private readonly GameObject _gameObject;
        private readonly Scene _scene;

        private CacheSpec(CacheBinding binding, GameObject gameObject, Scene scene)
        {
            _binding = binding;
            _gameObject = gameObject;
            _scene = scene;
        }

        public static CacheSpec Persistent =>
            new(CacheBinding.Persistent, null, default);

        public static CacheSpec ForGameObject(GameObject target)
        {
            if (!target)
                throw new ArgumentNullException(nameof(target),
                    "WithCache(GameObject) requires a live GameObject.");
            return new CacheSpec(CacheBinding.GameObject, target, default);
        }

        public static CacheSpec ForActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
                throw new InvalidOperationException(
                    "WithBindingScene() requires a valid active Scene.");
            return new CacheSpec(CacheBinding.Scene, null, scene);
        }

        public EventCache ToCache(object payload) => _binding switch
        {
            CacheBinding.GameObject => EventCache.BoundToGameObject(payload, _gameObject),
            CacheBinding.Scene => EventCache.BoundToScene(payload, _scene),
            _ => EventCache.Persistent(payload),
        };
    }
}
