namespace Gears5Helper;

public enum StoreChannel
{
    Shared,
    Steam,
    MicrosoftStore,
    Unspecified,
}

public enum GameTitle
{
    Gears5,
    EDay,
    Unknown,
}

public sealed class ConfigTarget
{
    public required string ConfigDirectory { get; init; }

    public required StoreChannel Channel { get; init; }

    public required GameTitle Title { get; init; }

    public string GameIniPath => Path.Combine(ConfigDirectory, "Game.ini");

    public string UserSettingsPath => Path.Combine(ConfigDirectory, "GameUserSettings.ini");

    public string ChannelText => Channel switch
    {
        StoreChannel.Steam => "Steam",
        StoreChannel.MicrosoftStore => Locale.Get("store"),
        StoreChannel.Shared => Locale.Get("shared"),
        _ => Locale.Get("unspecified"),
    };

    public string TitleText => Title switch
    {
        GameTitle.Gears5 => Locale.Get("gears5"),
        GameTitle.EDay => Locale.Get("eday"),
        _ => Locale.Get("unknown"),
    };

    public static StoreChannel ClassifyChannel(string path, GameTitle title)
    {
        if (ContainsSegment(path, "Steam"))
        {
            return StoreChannel.Steam;
        }

        if (ContainsSegment(path, "WinGDK") || ContainsSegment(path, "Packages") || ContainsSegment(path, "WindowsApps"))
        {
            return StoreChannel.MicrosoftStore;
        }

        return title == GameTitle.Gears5 ? StoreChannel.Shared : StoreChannel.Unspecified;
    }

    public static GameTitle ClassifyTitle(string path)
    {
        if (ContainsSegment(path, "Gears5") || path.Contains("Halifax", StringComparison.OrdinalIgnoreCase))
        {
            return GameTitle.Gears5;
        }

        if (path.Contains("E-Day", StringComparison.OrdinalIgnoreCase)
            || path.Contains("EDay", StringComparison.OrdinalIgnoreCase)
            || path.Contains("Fairlight", StringComparison.OrdinalIgnoreCase))
        {
            return GameTitle.EDay;
        }

        return GameTitle.Unknown;
    }

    public string DescribeStatus()
    {
        if (!Directory.Exists(ConfigDirectory))
        {
            return Locale.Get("noDir");
        }

        if (!File.Exists(GameIniPath))
        {
            return Locale.Get("noIni");
        }

        try
        {
            var (text, _) = IniFile.Read(GameIniPath);
            if (!text.Contains("UnrealCultureToWwiseCulture=", StringComparison.Ordinal))
            {
                return Locale.Get("unmodified");
            }

            if (!CultureMap.ContainsEnglishVoiceForChinese(text))
            {
                return Locale.Get("written");
            }

            if (Title != GameTitle.Gears5)
            {
                return Locale.Get("set");
            }

            if (!File.Exists(UserSettingsPath))
            {
                return Locale.Get("noCode");
            }

            var (settings, _) = IniFile.Read(UserSettingsPath);
            var code = IniFile.ReadKey(settings, CultureMap.PreferredLanguageKey);
            return string.Equals(code, CultureMap.Gears5ChineseSubtitleCode, StringComparison.OrdinalIgnoreCase)
                ? Locale.Get("set")
                : Locale.Format("langCode", string.IsNullOrEmpty(code) ? Locale.Get("empty") : code);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private static bool ContainsSegment(string path, string segment)
    {
        return path.Contains(Path.DirectorySeparatorChar + segment + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || path.Contains(Path.AltDirectorySeparatorChar + segment + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
