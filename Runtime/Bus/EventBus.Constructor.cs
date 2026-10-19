using AceLand.EventDriven.Bus.Services;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        private static SignatureService Signatures;
        private static CacheService Cache;
        private static RegistryService Registry;

        internal static void Initialize()
        {
            // Dispose the previous CacheService (if any) so its SceneManager.sceneUnloaded
            // subscription is released before a new one subscribes (no-domain-reload safe).
            Cache?.InternalDispose();

            Signatures = SignatureService.Build();
            Cache = CacheService.Build();
            Registry = RegistryService.Build(Signatures, Cache);

            Signatures.InitializeFromGenerated();
            Registry.BootstrapReset();
        }
    }
}