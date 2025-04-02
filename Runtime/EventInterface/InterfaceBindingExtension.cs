using System;

namespace AceLand.EventDriven.EventInterface
{
    [Obsolete("Please use EventBus instead. Please refer to the documentation: " +
              "https://docs.parsue.io/aceland-unity-packages/packages/event-driven/eventbus")]
    public static class InterfaceBindingExtension
    {
        public static void Bind<TInterface>(this object target) =>
            InterfaceBinding.Bind<TInterface>(target);
        
        public static void Bind<T0, T1>(this object target) =>
            InterfaceBinding.Bind<T0, T1>(target);
        
        public static void Bind<T0, T1, T2>(this object target) =>
            InterfaceBinding.Bind<T0, T1, T2>(target);
        
        public static void Bind<T0, T1, T2, T3>(this object target) =>
            InterfaceBinding.Bind<T0, T1, T2, T3>(target);
        
        public static void Bind<T0, T1, T2, T3, T4>(this object target) =>
            InterfaceBinding.Bind<T0, T1, T2, T3, T4>(target);
        
        public static void Unbind<TInterface>(this object target) =>
            InterfaceBinding.Unbind<TInterface>(target);
        
        public static void Unbind<T0, T1>(this object target) =>
            InterfaceBinding.Unbind<T0, T1>(target);
        
        public static void Unbind<T0, T1, T2>(this object target) =>
            InterfaceBinding.Unbind<T0, T1, T2>(target);
        
        public static void Unbind<T0, T1, T2, T3>(this object target) =>
            InterfaceBinding.Unbind<T0, T1, T2, T3>(target);
        
        public static void Unbind<T0, T1, T2, T3, T4>(this object target) =>
            InterfaceBinding.Unbind<T0, T1, T2, T3, T4>(target);
        
        public static bool Implements<TInterface>(this object target) =>
            InterfaceBinding.Implements<TInterface>(target);
    }
}