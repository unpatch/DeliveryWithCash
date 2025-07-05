using MelonLoader;

[assembly: MelonInfo(typeof(DeliveryWithCash.Core), "DeliveryWithCash", "1.0.0", "unpatch", "https://github.com/unpatch/DeliveryWithCash")]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonColor(0, 157, 255, 82)]
#if IL2CPP
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
#elif MONO
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
#endif


namespace DeliveryWithCash;

public class Core : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("DeliveryWithCash initialized.");
    }
}