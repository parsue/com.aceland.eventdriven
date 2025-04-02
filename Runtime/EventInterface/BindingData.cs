using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.ProjectSetting;
using AceLand.TaskUtils;
using UnityEngine;

namespace AceLand.EventDriven.EventInterface
{
    [Obsolete("Please use EventBus instead. Please refer to the documentation: " +
              "https://docs.parsue.io/aceland-unity-packages/packages/event-driven/eventbus")]
    internal class BindingData
    {
        private static EventDrivenSettings Settings => EventDrivenUtils.Settings;
        private readonly Dictionary<Type, List<object>> _bindings = new();
        
        public void Bind<TInterface>(object target)
        {
            var interfaceType = typeof(TInterface);
            if (!Validate(interfaceType, target)) return;

            if (!_bindings.ContainsKey(interfaceType))
                _bindings[interfaceType] = new List<object>();

            if (!_bindings[interfaceType].Contains(target))
                _bindings[interfaceType].Add(target);
        }

        public void Bind<T0, T1>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1) };
            
            BindMultiTargets(target, interfaceTypes);
        }

        public void Bind<T0, T1, T2>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1), typeof(T2) };

            BindMultiTargets(target, interfaceTypes);
        }

        public void Bind<T0, T1, T2, T3>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1), typeof(T2), typeof(T3) };

            BindMultiTargets(target, interfaceTypes);
        }

        public void Bind<T0, T1, T2, T3, T4>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4) };

            BindMultiTargets(target, interfaceTypes);
        }

        public void Unbind<TInterface>(object target)
        {
            var interfaceType = typeof(TInterface);
            if (!Validate(interfaceType, target)) return;
            if (!_bindings.TryGetValue(interfaceType, out var list)) return;
            if (!list.Contains(target)) return;
            list.Remove(target);
            if (list.Count > 0) return;
            _bindings.Remove(interfaceType);
        }

        public void Unbind<T0, T1>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1) };
            
            UnbindMultiTargets(target, interfaceTypes);
        }

        public void Unbind<T0, T1, T2>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1), typeof(T2) };

            UnbindMultiTargets(target, interfaceTypes);
        }

        public void Unbind<T0, T1, T2, T3>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1), typeof(T2), typeof(T3) };

            UnbindMultiTargets(target, interfaceTypes);
        }

        public void Unbind<T0, T1, T2, T3, T4>(object target)
        {
            var interfaceTypes = new[] 
                { typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4) };

            UnbindMultiTargets(target, interfaceTypes);
        }
        
        public bool Implements<TInterface>(object target)
        {
            var interfaceType = typeof(TInterface);
            if (!Validate(interfaceType, target) || !_bindings.TryGetValue(interfaceType, out var list))
                return false;
            
            return list.Contains(target);
        }
        
        public IEnumerable<TInterface> ListBindings<TInterface>()
        {
            var interfaceType = typeof(TInterface);
            if (!Validate(interfaceType))
                return Enumerable.Empty<TInterface>();

            if (_bindings.TryGetValue(interfaceType, out var list))
                return list.Cast<TInterface>();
            
            return Enumerable.Empty<TInterface>();
        }

        public Task<IEnumerable<TInterface>> ListBindingsAsync<TInterface>()
        {
            var token = Promise.ApplicationAliveToken;
            var interfaceType = typeof(TInterface);

            return Task.Run(async () =>
                {
                    if (!Validate(interfaceType)) return Enumerable.Empty<TInterface>();
                    
                    var targetTime = DateTime.Now.AddSeconds(Settings.BindingGetterTimeout);
                    while (!_bindings.ContainsKey(interfaceType) && DateTime.Now < targetTime &&
                           !token.IsCancellationRequested)
                        await Task.Delay(150, token);
                    
                    if (_bindings.TryGetValue(interfaceType, out var list))
                        return list.Cast<TInterface>();

                    return Enumerable.Empty<TInterface>();
                },
                token
            );
        }

        private void BindMultiTargets(object target, Type[] interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
                if (!Validate(interfaceType, target)) return;

            foreach (var interfaceType in interfaceTypes)
            {
                if (!_bindings.ContainsKey(interfaceType))
                    _bindings[interfaceType] = new List<object>();
                
                if (!_bindings[interfaceType].Contains(target))
                    _bindings[interfaceType].Add(target);
            }
        }

        private void UnbindMultiTargets(object target, Type[] interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
                if (!Validate(interfaceType, target)) return;

            foreach (var interfaceType in interfaceTypes)
            {
                if (!_bindings.TryGetValue(interfaceType, out var list)) continue;
                if (!list.Contains(target)) continue;
                list.Remove(target);
                if (list.Count > 0) continue;
                _bindings.Remove(interfaceType);
            }
        }

        private static bool Validate(Type interfaceType)
        {
            if (!Settings.IsAcceptedNamespace(interfaceType.FullName))
            {
                Debug.LogWarning($"Namespace '{interfaceType.FullName}' is not accepted.");
                return false;
            }
            
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException($"'{interfaceType.Name}' is not an interface type.");

            return true;
        }

        private static bool Validate(Type interfaceType, object target)
        {
            Validate(interfaceType);

            if (!interfaceType.IsAssignableFrom(target.GetType()))
                throw new InvalidOperationException(
                    $"Object of type '{target.GetType().Name}' does not implement the interface '{interfaceType.Name}'.");
            
            return true;
        }
    }
}