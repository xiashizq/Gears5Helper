namespace Gears5Helper;

public static class LanguagePatch
{
    public const string BackupSuffix = ".gears5helper.bak";

    public const string CreatedMarkerSuffix = ".gears5helper.created";

    public static IReadOnlyList<string> Apply(
        ConfigTarget target,
        IReadOnlyDictionary<string, string>? voices = null,
        string? interfaceCode = null)
    {
        Directory.CreateDirectory(target.ConfigDirectory);
        var map = voices ?? CultureMap.DefaultVoices();
        var messages = new List<string>
        {
            PatchGameIni(target.GameIniPath, map),
        };

        if (target.Title == GameTitle.Gears5)
        {
            var code = interfaceCode == null
                ? CultureMap.Gears5ChineseSubtitleCode
                : CultureMap.ToGears5Code(interfaceCode, map);
            messages.Add(PatchGears5Language(target.UserSettingsPath, code));
        }

        return messages;
    }

    public static IReadOnlyList<string> Restore(ConfigTarget target)
    {
        var messages = new List<string>
        {
            RestoreFile(target.GameIniPath),
        };

        if (target.Title == GameTitle.Gears5)
        {
            messages.Add(RestoreFile(target.UserSettingsPath));
        }

        return messages;
    }

    public static string PatchGameIni(string path, IReadOnlyDictionary<string, string>? voices = null)
    {
        var body = new[] { CultureMap.BuildLine(voices ?? CultureMap.DefaultVoices()) };
        if (!File.Exists(path))
        {
            var created = IniFile.ReplaceSection(string.Empty, CultureMap.AkSectionHeader, body);
            IniFile.WriteAtomic(path, created, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            File.WriteAllText(path + CreatedMarkerSuffix, string.Empty);
            return Locale.Format("created", path);
        }

        BackupOnce(path);
        var (text, encoding) = IniFile.Read(path);
        var updated = IniFile.ReplaceSection(text, CultureMap.AkSectionHeader, body);
        IniFile.WriteAtomic(path, updated, encoding);
        return Locale.Format("updated", path);
    }

    public static string PatchGears5Language(string path, string? languageCode = null)
    {
        var code = string.IsNullOrWhiteSpace(languageCode) ? CultureMap.Gears5ChineseSubtitleCode : languageCode;
        if (!File.Exists(path))
        {
            var created = IniFile.SetKey(
                string.Empty,
                CultureMap.Gears5LanguageSection,
                CultureMap.PreferredLanguageKey,
                code);
            IniFile.WriteAtomic(path, created, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            File.WriteAllText(path + CreatedMarkerSuffix, string.Empty);
            return Locale.Format("createdCode", path, code);
        }

        BackupOnce(path);
        var (text, encoding) = IniFile.Read(path);
        var updated = IniFile.SetKey(
            text,
            CultureMap.Gears5LanguageSection,
            CultureMap.PreferredLanguageKey,
            code);
        IniFile.WriteAtomic(path, updated, encoding);
        return "PreferredLanguageCode=" + code + " " + path;
    }

    public static string RestoreFile(string path)
    {
        var backup = path + BackupSuffix;
        var marker = path + CreatedMarkerSuffix;
        if (File.Exists(backup))
        {
            ClearReadOnly(path);
            File.Copy(backup, path, overwrite: true);
            return Locale.Format("restored", path);
        }

        if (File.Exists(marker))
        {
            if (File.Exists(path))
            {
                ClearReadOnly(path);
                File.Delete(path);
            }

            File.Delete(marker);
            return Locale.Format("deleted", path);
        }

        return Locale.Format("noBackup", path);
    }

    private static void BackupOnce(string path)
    {
        var backup = path + BackupSuffix;
        if (!File.Exists(backup))
        {
            File.Copy(path, backup);
        }
    }

    private static void ClearReadOnly(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReadOnly) != 0)
        {
            File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
        }
    }
}
