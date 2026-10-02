using System.Text;

namespace Gears5Helper;

public static class IniFile
{
    public static (string Text, Encoding Encoding) Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var encoding = DetectEncoding(bytes);
        var preamble = encoding.GetPreamble();
        var offset = 0;
        if (preamble.Length > 0 && bytes.Length >= preamble.Length && StartsWith(bytes, preamble))
        {
            offset = preamble.Length;
        }

        return (encoding.GetString(bytes, offset, bytes.Length - offset), encoding);
    }

    public static void WriteAtomic(string path, string text, Encoding encoding)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        ClearReadOnly(path);
        var temporary = path + ".tmp";
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var writer = new StreamWriter(stream, encoding))
        {
            writer.Write(text);
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        File.Move(temporary, path);
    }

    public static string ReplaceSection(string content, string sectionHeader, IReadOnlyList<string> bodyLines)
    {
        var newline = DetectNewline(content);
        var lines = SplitLines(content);
        var replacement = new List<string> { sectionHeader };
        replacement.AddRange(bodyLines);

        var found = false;
        for (var index = 0; index < lines.Count;)
        {
            if (!LineIsSection(lines[index], sectionHeader))
            {
                index++;
                continue;
            }

            var end = index + 1;
            while (end < lines.Count && !IsSectionLine(lines[end]))
            {
                end++;
            }

            lines.RemoveRange(index, end - index);
            lines.InsertRange(index, replacement);
            index += replacement.Count;
            found = true;
        }

        if (!found)
        {
            if (lines.Count > 0 && lines[^1].Length > 0)
            {
                lines.Add(string.Empty);
            }

            lines.AddRange(replacement);
        }

        var joined = string.Join(newline, lines);
        if (!joined.EndsWith(newline, StringComparison.Ordinal))
        {
            joined += newline;
        }

        return joined;
    }

    public static string SetKey(string content, string sectionHeader, string key, string value)
    {
        var newline = DetectNewline(content);
        var lines = SplitLines(content);
        var assignment = key + "=" + value;
        var replaced = false;

        for (var index = 0; index < lines.Count; index++)
        {
            if (TryReadKey(lines[index], out var existingKey, out _) &&
                existingKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                var padding = lines[index].Length - lines[index].TrimStart().Length;
                lines[index] = new string(' ', padding) + assignment;
                replaced = true;
            }
        }

        if (replaced)
        {
            return Join(lines, newline, content);
        }

        var sectionIndex = FindSection(lines, sectionHeader);
        if (sectionIndex >= 0)
        {
            var insertAt = sectionIndex + 1;
            while (insertAt < lines.Count && !IsSectionLine(lines[insertAt]))
            {
                insertAt++;
            }

            lines.Insert(insertAt, assignment);
            return Join(lines, newline, content);
        }

        if (lines.Count > 0 && lines[^1].Length > 0)
        {
            lines.Add(string.Empty);
        }

        lines.Add(sectionHeader);
        lines.Add(assignment);
        return Join(lines, newline, content);
    }

    public static string? ReadKey(string content, string key)
    {
        foreach (var line in SplitLines(content))
        {
            if (TryReadKey(line, out var existingKey, out var existingValue) &&
                existingKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return existingValue;
            }
        }

        return null;
    }

    public static Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
        }

        if (LooksLikeUtf16Le(bytes))
        {
            return new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }

    private static bool LooksLikeUtf16Le(byte[] bytes)
    {
        var sample = Math.Min(bytes.Length, 256);
        if (sample < 8)
        {
            return false;
        }

        var pairs = 0;
        var zeroHighBytes = 0;
        for (var index = 0; index + 1 < sample; index += 2)
        {
            pairs++;
            if (bytes[index + 1] == 0 && bytes[index] != 0)
            {
                zeroHighBytes++;
            }
        }

        return pairs > 0 && zeroHighBytes * 10 >= pairs * 8;
    }

    private static bool StartsWith(byte[] bytes, byte[] prefix)
    {
        for (var index = 0; index < prefix.Length; index++)
        {
            if (bytes[index] != prefix[index])
            {
                return false;
            }
        }

        return true;
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

    private static string DetectNewline(string content)
    {
        if (content.Contains("\r\n", StringComparison.Ordinal))
        {
            return "\r\n";
        }

        if (content.Contains('\n'))
        {
            return "\n";
        }

        return "\r\n";
    }

    private static List<string> SplitLines(string content)
    {
        if (content.Length == 0)
        {
            return [];
        }

        var normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var parts = normalized.Split('\n');
        if (parts.Length > 0 && parts[^1].Length == 0)
        {
            return parts.Take(parts.Length - 1).ToList();
        }

        return parts.ToList();
    }

    private static string Join(List<string> lines, string newline, string original)
    {
        var joined = string.Join(newline, lines);
        if (original.Length == 0 || original.EndsWith('\n'))
        {
            if (!joined.EndsWith(newline, StringComparison.Ordinal))
            {
                joined += newline;
            }
        }

        return joined;
    }

    private static int FindSection(List<string> lines, string sectionHeader)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            if (LineIsSection(lines[index], sectionHeader) || SectionNameMatches(lines[index], sectionHeader))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool SectionNameMatches(string line, string sectionHeader)
    {
        var trimmed = line.Trim();
        if (trimmed.Length < 3 || trimmed[0] != '[')
        {
            return false;
        }

        var close = trimmed.IndexOf(']');
        if (close <= 1)
        {
            return false;
        }

        var name = trimmed[1..close];
        var wanted = sectionHeader.Trim();
        if (wanted.StartsWith('[') && wanted.EndsWith(']') && wanted.Length >= 2)
        {
            wanted = wanted[1..^1];
        }

        return name.Equals(wanted, StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("." + wanted, StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(wanted, StringComparison.OrdinalIgnoreCase);
    }

    private static bool LineIsSection(string line, string sectionHeader)
    {
        return line.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSectionLine(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith('[') && trimmed.Contains(']');
    }

    private static bool TryReadKey(string line, out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith(';') || trimmed.StartsWith('['))
        {
            return false;
        }

        var separator = trimmed.IndexOf('=');
        if (separator <= 0)
        {
            return false;
        }

        key = trimmed[..separator].Trim();
        value = trimmed[(separator + 1)..].Trim();
        return key.Length > 0;
    }
}
