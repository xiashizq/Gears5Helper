namespace Gears5Helper;

public static class ConfigScanner
{
    private static readonly string[] ConfigLeafNames = ["Windows", "PC", "WindowsNoEditor", "WinGDK", "Win64"];

    private static readonly HashSet<string> SkippedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Logs", "Cache", "Crashes", "SaveGames", "Screenshots", "Telemetry",
        "D3DDriverCache", "ShaderCache", "Intermediate", "Binaries", "Content",
    };

    public static IReadOnlyList<ConfigTarget> Scan(string localAppData)
    {
        var results = new List<ConfigTarget>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(localAppData))
        {
            return results;
        }

        Walk(Path.Combine(localAppData, "Gears5"), 0, directory => Add(results, seen, directory));
        WalkMatchingChildren(Path.Combine(localAppData, "Microsoft"), IsMicrosoftGameFolder, directory => Add(results, seen, directory));
        WalkMatchingChildren(Path.Combine(localAppData, "Packages"), IsPackageFolder, directory => Add(results, seen, directory));
        Walk(Path.Combine(localAppData, "GearsEDay"), 0, directory => Add(results, seen, directory));
        Walk(Path.Combine(localAppData, "Gears of War E-Day"), 0, directory => Add(results, seen, directory));
        AddPackageSettings(localAppData, results, seen);

        return results;
    }

    public static IReadOnlyList<ConfigTarget> FromUserFolder(string selectedPath)
    {
        if (string.IsNullOrWhiteSpace(selectedPath) || !Directory.Exists(selectedPath))
        {
            return [];
        }

        var full = Path.GetFullPath(selectedPath);
        if (IsConfigLeaf(full))
        {
            return [Create(full)];
        }

        var found = new List<string>();
        Walk(full, 0, found.Add);
        if (found.Count > 0)
        {
            return found.Select(Create).ToList();
        }

        if (File.Exists(Path.Combine(full, "Game.ini")) || File.Exists(Path.Combine(full, "GameUserSettings.ini")))
        {
            return [Create(full)];
        }

        return [];
    }

    private static void AddPackageSettings(string localAppData, List<ConfigTarget> results, HashSet<string> seen)
    {
        var packages = Path.Combine(localAppData, "Packages");
        if (!Directory.Exists(packages))
        {
            return;
        }

        IEnumerable<string> children;
        try
        {
            children = Directory.EnumerateDirectories(packages);
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        foreach (var package in children)
        {
            if (!IsPackageFolder(Path.GetFileName(package)))
            {
                continue;
            }

            var settings = Path.Combine(package, "Settings");
            if (File.Exists(Path.Combine(settings, "Game.ini")) || File.Exists(Path.Combine(settings, "GameUserSettings.ini")))
            {
                Add(results, seen, settings);
            }
        }
    }

    private static void WalkMatchingChildren(string parent, Func<string, bool> match, Action<string> onConfigDir)
    {
        if (!Directory.Exists(parent))
        {
            return;
        }

        IEnumerable<string> children;
        try
        {
            children = Directory.EnumerateDirectories(parent);
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        foreach (var child in children)
        {
            if (match(Path.GetFileName(child)))
            {
                Walk(child, 0, onConfigDir);
            }
        }
    }

    private static void Walk(string directory, int depth, Action<string> onConfigDir)
    {
        if (depth > 8 || !Directory.Exists(directory))
        {
            return;
        }

        if (IsConfigLeaf(directory))
        {
            onConfigDir(directory);
            return;
        }

        IEnumerable<string> children;
        try
        {
            children = Directory.EnumerateDirectories(directory);
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        var name = Path.GetFileName(directory);
        foreach (var child in children)
        {
            var childName = Path.GetFileName(child);
            if (name.Equals("Saved", StringComparison.OrdinalIgnoreCase)
                && !childName.Equals("Config", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (SkippedDirectories.Contains(childName))
            {
                continue;
            }

            Walk(child, depth + 1, onConfigDir);
        }
    }

    private static bool IsConfigLeaf(string directory)
    {
        var name = Path.GetFileName(directory);
        if (!ConfigLeafNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var parent = Path.GetDirectoryName(directory);
        var grand = parent == null ? null : Path.GetDirectoryName(parent);
        return parent != null
            && grand != null
            && Path.GetFileName(parent).Equals("Config", StringComparison.OrdinalIgnoreCase)
            && Path.GetFileName(grand).Equals("Saved", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMicrosoftGameFolder(string name) =>
        name.Contains("Gears", StringComparison.OrdinalIgnoreCase)
        || name.Contains("Fairlight", StringComparison.OrdinalIgnoreCase)
        || name.Contains("EDay", StringComparison.OrdinalIgnoreCase)
        || name.Contains("E-Day", StringComparison.OrdinalIgnoreCase);

    private static bool IsPackageFolder(string name) =>
        name.Contains("Halifax", StringComparison.OrdinalIgnoreCase)
        || name.Contains("Gears", StringComparison.OrdinalIgnoreCase)
        || name.Contains("Fairlight", StringComparison.OrdinalIgnoreCase)
        || name.Contains("EDay", StringComparison.OrdinalIgnoreCase)
        || name.Contains("E-Day", StringComparison.OrdinalIgnoreCase);

    private static void Add(List<ConfigTarget> results, HashSet<string> seen, string directory)
    {
        string full;
        try
        {
            full = Path.GetFullPath(directory);
        }
        catch (Exception)
        {
            return;
        }

        if (!seen.Add(full))
        {
            return;
        }

        results.Add(Create(full));
    }

    private static ConfigTarget Create(string directory)
    {
        var title = ConfigTarget.ClassifyTitle(directory);
        return new ConfigTarget
        {
            ConfigDirectory = directory,
            Title = title,
            Channel = ConfigTarget.ClassifyChannel(directory, title),
        };
    }
}
