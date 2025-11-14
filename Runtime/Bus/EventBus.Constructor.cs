using AceLand.EventDriven.Bus.Services;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        private static SignatureService Signatures;
        private static CacheService Cache;
        private static RegistryService Registry;

        public static void Initialize()
        {
            Signatures = SignatureService.Build();
            Cache = CacheService.Build();
            Registry = RegistryService.Build(Signatures, Cache);

            Signatures.InitializeAndScan();
            Registry.BootstrapReset();
        }
    }
}