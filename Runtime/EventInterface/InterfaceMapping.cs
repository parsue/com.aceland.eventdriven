using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.ProjectSetting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AceLand.EventDriven.EventInterface
{
    public static class InterfaceMapping
    {
        private static EventDrivenSettings Settings => EventDrivenUtils.Settings;
        private static Dictionary<Type, Type[]> _interfaceComponentsMapping;
        
        private static bool Initialized { get; set; }

        internal static async void Initialization()
        {
            Debug.Log("Interface Mapping Initializing ...");
            if (Settings.AcceptedNamespaceCount > 0)
            {
                _interfaceComponentsMapping = new Dictionary<Type, Type[]>();
                await SetupMapping();
                Debug.Log("Interface Mapping Initialized");
            }
            else
            {
                Debug.LogWarning("Interface Mapping not initialized: no accepted namespace");
            }
            
            Initialized = true;
        }

        private static Task SetupMapping()
        {
            return Task.Run(() =>
            {
                var typesSpan = GetAllTypesFromAssemblies();
                foreach (var type in typesSpan)
                {
                    if (!type.IsInterface) continue;

                    var name = type.ToString().ToLower();
                    if (!IsAcceptedType(name)) continue;
                    
                    var componentsList = new List<Type>();
                    foreach (var curType in typesSpan)
                    {
                        var t = typeof(Component);
                        if (curType.IsInterface) continue;
                        if (!type.IsAssignableFrom(curType) || !curType.IsSubclassOf(t)) continue;
                        if (t != curType && !curType.IsSubclassOf(t)) continue;

                        if (!componentsList.Contains(curType))
                            componentsList.Add(curType);
                    }

                    _interfaceComponentsMapping.Add(type, componentsList.ToArray());
                }
            });
        }

        private static bool IsAcceptedType(string typeName)
        {
            foreach (var acceptedName in Settings.AcceptedNamespaces)
            {
                if (!typeName.StartsWith(acceptedName.ToLower())) continue;
                return true;
            }
            return false;
        }

        private static ReadOnlySpan<Type> GetAllTypesFromAssemblies()
        {
            ReadOnlySpan<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                ReadOnlySpan<Type> types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var typeName = type.ToString().ToLower();
                    if (!IsAcceptedType(typeName)) continue;
                    allTypes.Add(type);
                }
            }

            ReadOnlySpan<Type> allTypesSpan = allTypes.ToArray();
            return allTypesSpan;
        }
        
        public static IEnumerable<T> FindObjects<T>() where T : class
        {
            if (!Initialized) yield break;
            
            var t = typeof(T);
            if (!t.IsInterface)
            {
                throw new Exception("InterfaceFunction.FindObjects error: not interface");
            }
            if (!_interfaceComponentsMapping.TryGetValue(t, out var types)) yield break;

            if (types.Length == 0) yield break;

            foreach (var curType in types)
            {
                var objects = Object.FindObjectsByType(curType, FindObjectsSortMode.None);
                if (objects.Length == 0) continue;

                foreach (var curObj in objects)
                {
                    if (curObj is not T curObjAsT)
                        throw new Exception($"Unable to cast '{curObj.GetType()}' to '{t}'");
                    
                    yield return curObjAsT;
                }
            }
        }

        public static T FindObject<T>() where T : class
        {
            if (!Initialized) return null;
            
            var t = typeof(T);
            if (!t.IsInterface) return null;
            if (!_interfaceComponentsMapping.TryGetValue(t, out var types1)) return null;

            ReadOnlySpan<Type> types = types1;
            if (types.IsEmpty) return null;
            return Object.FindFirstObjectByType(types[0]) as T;
        }

        public static IEnumerable<T> GetInterfaceComponents<T>(this Component component) where T : class
        {
            if (!Initialized) yield break;

            var t = typeof(T);
            if (!t.IsInterface)
            {
                throw new Exception("InterfaceFunction.FindObjects error: not interface");
            }
            if (!_interfaceComponentsMapping.TryGetValue(t, out var types)) yield break;

            if (types.Length == 0) yield break;

            foreach (var curType in types)
            {
                var components = component.GetComponents(curType);
                if (components.Length == 0) continue;

                foreach (var curComp in components)
                {
                    if (curComp is not T curCompAsT)
                    {
                        Debug.LogError("Unable to cast '" + curComp.GetType() + "' to '" + t + "'");
                        continue;
                    }
                    yield return curCompAsT;
                }
            }
        }

        public static T GetInterfaceComponent<T>(this Component component) where T : class
        {
            if (!Initialized) return null;

            var t = typeof(T);
            if (!t.IsInterface) return null;
            if (!_interfaceComponentsMapping.TryGetValue(t, out var types1)) return null;

            ReadOnlySpan<Type> types = types1;
            if (types.IsEmpty) return null;

            return component.GetComponent(types[0]) as T;
        }
    }
}