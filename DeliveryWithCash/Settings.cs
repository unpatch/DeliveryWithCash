using MelonLoader;
using MelonLoader.Utils;

namespace DeliveryWithCash;

public static class Settings
{
    public enum EKeys
    {
        OnlyCash,
    }

    public static void LoadSettings(Core core)
    {
        m_category = MelonPreferences.CreateCategory(core.Info.Name);
        m_category.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, $"{core.Info.Name}.cfg"));

        foreach (EKeys key in Enum.GetValues(typeof(EKeys)))
        {
            switch (key)
            {
                case EKeys.OnlyCash:
                    m_category.CreateEntry<bool>(key.GetString(), false);
                    break;
            }
        }
    }

    public static T Get<T>(EKeys key)
    {
        T value = m_category.GetEntry<T>(key.GetString()).Value;
        return value;
    }

    private static string GetString(this EKeys key)
    {
        string str = key.ToString();
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    private static MelonPreferences_Category m_category;
}