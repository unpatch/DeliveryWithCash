using MelonLoader;
using MelonLoader.Utils;

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
    public static bool MoreVans { get; private set; } = false;

    private static MelonPreferences_Category m_category;

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg($"{Info.Name} initialized.");

        LoadSettings();
    }

    private void LoadSettings()
    {
        m_category = MelonPreferences.CreateCategory(Info.Name);
        m_category.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, $"{Info.Name}.cfg"));
        m_category.CreateEntry<bool>("moreVans", false);
        MoreVans = m_category.GetEntry<bool>("moreVans").Value;
        LoggerInstance.Msg($"MoreVans = {MoreVans}");
    }
}