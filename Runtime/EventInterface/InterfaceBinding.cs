using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AceLand.EventDriven.EventInterface
{
    public static class InterfaceBinding
    {
        private static BindingData _bindingData;

        internal static void Initialization()
        {
            _bindingData = new BindingData();
        }
        
        public static void Bind<TInterface>(object target) =>
            _bindingData.Bind<TInterface>(target);
        
        public static void Unbind<TInterface>(object target) =>
            _bindingData.Unbind<TInterface>(target);
        
        public static bool Implements<TInterface>(object target) =>
            _bindingData.Implements<TInterface>(target);
        
        public static ReadOnlySpan<TInterface> ListBindings<TInterface>() =>
            _bindingData.ListBindings<TInterface>();
        
        public static Task<IEnumerable<TInterface>> ListBindingsAsync<TInterface>() =>
            _bindingData.ListBindingsAsync<TInterface>();
    }
}