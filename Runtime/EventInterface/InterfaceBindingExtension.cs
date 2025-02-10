using System.Collections.Generic;

namespace AceLand.EventDriven.EventInterface
{
    public static class InterfaceBindingExtension
    {
        public static void Bind<TInterface>(this object target) =>
            InterfaceBinding.Bind<TInterface>(target);
    }
}