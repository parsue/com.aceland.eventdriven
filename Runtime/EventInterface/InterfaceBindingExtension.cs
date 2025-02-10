using System.Collections.Generic;

namespace AceLand.EventDriven.EventInterface
{
    public static class InterfaceBindingExtension
    {
        public static void Bind<TInterface>(this object target) =>
            InterfaceBinding.Bind<TInterface>(target);
        
        public static void Unbind<TInterface>(this object target) =>
            InterfaceBinding.Unbind<TInterface>(target);
        
        public static bool Implements<TInterface>(this object target) =>
            InterfaceBinding.Implements<TInterface>(target);
    }
}