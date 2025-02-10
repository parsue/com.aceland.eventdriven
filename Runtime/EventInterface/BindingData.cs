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
        
        public bool Implements<TInterface>(object target)
        {
            var interfaceType = typeof(TInterface);
            if (!Validate(interfaceType, target)) return false;
            
            return _bindings.ContainsKey(interfaceType) && _bindings[interfaceType].Contains(target);
        }
        
        public IEnumerable<TInterface> ListBindings<TInterface>()
        {
            var interfaceType = typeof(TInterface);
            if (!Validate(interfaceType)) return Array.Empty<TInterface>();
            
            return _bindings[interfaceType].Cast<TInterface>();
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
                    return _bindings[interfaceType].Cast<TInterface>();
                },
                token
            );
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