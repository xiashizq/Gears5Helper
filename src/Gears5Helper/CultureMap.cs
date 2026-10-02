using System.Text.RegularExpressions;

namespace Gears5Helper;

public sealed record CultureRow(string Code, string DefaultVoice);

public static class CultureMap
{
    public const string AkSectionHeader = "[/Script/AkAudio.AkSettings]";

    public const string Gears5LanguageSection = "[GearGameUserSettings]";

    public const string PreferredLanguageKey = "PreferredLanguageCode";

    public const string Gears5ChineseSubtitleCode = "zh-HK";

    public static readonly string[] Voices =
    [
        "English(US)",
        "French(France)",
        "German",
        "Italian",
        "Korean",
        "Portuguese(Brazil)",
        "Spanish(Mexico)",
        "Spanish(Spain)",
        "Japanese",
    ];

    public static readonly CultureRow[] Rows =
    [
        new("en", "English(US)"),
        new("fr", "French(France)"),
        new("de", "German"),
        new("it", "Italian"),
        new("ko", "Korean"),
        new("zh-Hant", "English(US)"),
        new("pt-BR", "Portuguese(Brazil)"),
        new("es-419", "Spanish(Mexico)"),
        new("es-ES", "Spanish(Spain)"),
        new("zh-Hans", "English(US)"),
        new("ja", "Japanese"),
    ];

    private static readonly Regex PairPattern = new(
        @"\(\s*""([^""]*)""\s*,\s*""([^""]*)""\s*\)",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string CultureLine => BuildLine(DefaultVoices());

    public static Dictionary<string, string> DefaultVoices()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in Rows)
        {
            map[row.Code] = row.DefaultVoice;
        }

        return map;
    }

    public static Dictionary<string, string> Parse(string iniText)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in PairPattern.Matches(iniText))
        {
            map[match.Groups[1].Value] = match.Groups[2].Value;
        }

        return map;
    }

    public static Dictionary<string, string> Merge(IReadOnlyDictionary<string, string>? overlay)
    {
        var map = DefaultVoices();
        if (overlay == null)
        {
            return map;
        }

        foreach (var pair in overlay)
        {
            map[pair.Key] = pair.Value;
        }

        return map;
    }

    public static string BuildLine(IReadOnlyDictionary<string, string> voices)
    {
        var pairs = new List<string>();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in Rows)
        {
            var voice = voices.TryGetValue(row.Code, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : row.DefaultVoice;
            pairs.Add("(\"" + row.Code + "\", \"" + voice + "\")");
            used.Add(row.Code);
        }

        foreach (var extra in voices.Keys.Where(key => !used.Contains(key)).OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            pairs.Add("(\"" + extra + "\", \"" + voices[extra] + "\")");
        }

        return "UnrealCultureToWwiseCulture=(" + string.Join(",", pairs) + ")";
    }

    public static string ToGears5Code(string interfaceCode, IReadOnlyDictionary<string, string> voices)
    {
        if (interfaceCode.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase)
            && voices.TryGetValue("zh-Hant", out var voice)
            && voice.Equals("English(US)", StringComparison.OrdinalIgnoreCase))
        {
            return Gears5ChineseSubtitleCode;
        }

        return interfaceCode;
    }

    public static bool ContainsEnglishVoiceForChinese(string iniText) =>
        iniText.Contains("(\"zh-Hans\", \"English(US)\")", StringComparison.Ordinal)
        && iniText.Contains("(\"zh-Hant\", \"English(US)\")", StringComparison.Ordinal);
}
