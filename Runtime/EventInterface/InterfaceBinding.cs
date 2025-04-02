using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AceLand.EventDriven.EventInterface
{
    [Obsolete("Please use EventBus instead. Please refer to the documentation: " +
              "https://docs.parsue.io/aceland-unity-packages/packages/event-driven/eventbus")]
    public static class InterfaceBinding
    {
        private static BindingData _bindingData;

        internal static void Initialization()
        {
            _bindingData = new BindingData();
        }
        
        public static void Bind<TInterface>(object target) =>
            _bindingData.Bind<TInterface>(target);
        
        public static void Bind<T0, T1>(object target) =>
            _bindingData.Bind<T0, T1>(target);
        
        public static void Bind<T0, T1, T2>(object target) =>
            _bindingData.Bind<T0, T1, T2>(target);
        
        public static void Bind<T0, T1, T2, T3>(object target) =>
            _bindingData.Bind<T0, T1, T2, T3>(target);
        
        public static void Bind<T0, T1, T2, T3, T4>(object target) =>
            _bindingData.Bind<T0, T1, T2, T3, T4>(target);
        
        public static void Unbind<TInterface>(object target) =>
            _bindingData.Unbind<TInterface>(target);
        
        public static void Unbind<T0, T1>(object target) =>
            _bindingData.Unbind<T0, T1>(target);
        
        public static void Unbind<T0, T1, T2>(object target) =>
            _bindingData.Unbind<T0, T1, T2>(target);
        
        public static void Unbind<T0, T1, T2, T3>(object target) =>
            _bindingData.Unbind<T0, T1, T2, T3>(target);
        
        public static void Unbind<T0, T1, T2, T3, T4>(object target) =>
            _bindingData.Unbind<T0, T1, T2, T3, T4>(target);
        
        public static bool Implements<TInterface>(object target) =>
            _bindingData.Implements<TInterface>(target);
        
        public static IEnumerable<TInterface> ListBindings<TInterface>() =>
            _bindingData.ListBindings<TInterface>();
        
        public static Task<IEnumerable<TInterface>> ListBindingsAsync<TInterface>() =>
            _bindingData.ListBindingsAsync<TInterface>();
    }
}