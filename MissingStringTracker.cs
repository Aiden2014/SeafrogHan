using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;

namespace SeafrogHan;

/// <summary>
/// 追踪和记录缺失的翻译字符串
/// </summary>
public static class MissingStringTracker
{
    private static readonly string csvMissingFilePath = Path.Combine(
        Paths.PluginPath,
        "resources",
        "tmp_m_text_missing.csv"
    );
    private static readonly HashSet<string> existingTmpMText = [];
    private static bool isInitialized = false;

    public static void Initialize()
    {
        if (isInitialized)
            return;

        try
        {
            InitMissingStrings(csvMissingFilePath, existingTmpMText);
            isInitialized = true;
            Plugin.Log.LogInfo("Missing string tracker initialized");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to initialize missing string tracker: {ex.Message}");
        }
    }

    private static void InitMissingStrings(string filePath, HashSet<string> missingStrings)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            var records = CsvHelper.ParseCsvRecords(content);
            foreach (var fields in records)
            {
                if (fields.Length > 1)
                {
                    string innerKey = CsvHelper.NormalizeLineEndingsToCrlf(fields[0]);
                    missingStrings.Add(innerKey);
                    Plugin.Log.LogInfo($"Loaded missing string: {innerKey}");
                }
            }
            Plugin.Log.LogInfo($"Loaded {missingStrings.Count} entries from {Path.GetFileName(filePath)}");
        }
    }

    public static void RecordMissingTmpMText(string key, string text)
    {
        SaveMissingString(key, text, csvMissingFilePath, existingTmpMText);
    }

    public static bool IsIgnoreMissingTmpMText(string key, string text)
    {
        string normalizedKey = CsvHelper.NormalizeLineEndingsToCrlf(key);
        return string.IsNullOrEmpty(text)
            || ConatinsChinese(text)
            || IsNumberAndSymbol(text)
            || IsResolution(text)
            || IsChipsFound(text)
            || IsNumberXNumber(text)
            || IsNumberText(text);
    }

    private static bool IsKnownMissingKey(string normalizedKey)
    {
        if (existingTmpMText.Contains(normalizedKey))
        {
            return true;
        }

        string trimmedKey = CsvHelper.TrimTrailingLineEndings(normalizedKey);
        return existingTmpMText.Contains(trimmedKey);
    }

    public static bool IsNumberAndSymbol(string text)
    {
        foreach (char c in text)
        {
            if (!char.IsDigit(c) && !char.IsPunctuation(c) && !char.IsSymbol(c) && !char.IsWhiteSpace(c))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsResolution(string text)
    {
        // 判断是否为分辨率字符串，示例格式 "2560x1440 @ 99hz,2560x1440 @ 99hz"
        Regex resolutionPattern = new Regex(@"^\d{3,5}x\d{3,5} @ \d{2,3}hz(,\d{3,5}x\d{3,5} @ \d{2,3}hz)*$");
        return resolutionPattern.IsMatch(text);
    }

    private static bool IsNumberXNumber(string text)
    {
        // 判断是否为 "数字x数字" 格式，例如 "3 x 3"
        Regex pattern = new Regex(@"^\d+ x \d+$");
        return pattern.IsMatch(text);
    }

    public static bool IsChipsFound(string text)
    {
        // 判断是否为 0/2 P.O.T.A.T.O Chips Found 这种格式
        Regex pattern = new Regex(@"^\d+/\d+ P\.O\.T\.A\.T\.O Chips Found$");
        return pattern.IsMatch(text);
    }

    private static bool IsNumberText(string text)
    {
        // 判断是否为 x<size=150%>3</size> 和 x<size=150%>2</size> 这种格式
        Regex pattern = new Regex(@"^x<size=\d+%>\d+</size>$");
        return pattern.IsMatch(text);
    }

    private static bool ConatinsChinese(string text)
    {
        foreach (char c in text)
        {
            if (c >= 0x4E00 && c <= 0x9FFF)
            {
                return true;
            }
        }
        return false;
    }

    private static void SaveMissingString(string key, string text, string filePath, HashSet<string> missingStrings)
    {
        string normalizedKey = CsvHelper.NormalizeLineEndingsToCrlf(key);
        string normalizedText = CsvHelper.NormalizeLineEndingsToCrlf(text);

        if (string.IsNullOrEmpty(normalizedKey) || IsKnownMissingKey(normalizedKey))
        {
            return;
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true, System.Text.Encoding.UTF8))
            {
                writer.NewLine = CsvHelper.Crlf;
                string escapedKey = CsvHelper.EscapeCsvValue(normalizedKey);
                string escapedText = CsvHelper.EscapeCsvValue(normalizedText);
                writer.WriteLine($"{escapedKey},{escapedText}");
            }

            missingStrings.Add(normalizedKey);
            missingStrings.Add(CsvHelper.TrimTrailingLineEndings(normalizedKey));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to write to {Path.GetFileName(filePath)}: {ex.Message}");
        }
    }
}

/// <summary>
/// CSV 格式处理辅助类
/// </summary>
public static class CsvHelper
{
    public const string Crlf = "\r\n";

    public static string NormalizeLineEndingsToCrlf(string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Crlf);
    }

    public static string TrimTrailingLineEndings(string value)
    {
        string normalized = NormalizeLineEndingsToCrlf(value);
        while (normalized.EndsWith(Crlf, StringComparison.Ordinal))
        {
            normalized = normalized.Substring(0, normalized.Length - Crlf.Length);
        }

        return normalized;
    }

    public static string EscapeCsvValue(string value)
    {
        string escaped = value?.Replace("\"", "\"\"") ?? "";
        if (escaped.Contains(",") || escaped.Contains("\"") || escaped.Contains("\n"))
        {
            escaped = $"\"{escaped}\"";
        }
        return escaped;
    }

    /// <summary>
    /// 解析 CSV 内容为记录列表，正确处理多行引号字段（不会像 TextFieldParser 那样丢失空行）
    /// </summary>
    public static List<string[]> ParseCsvRecords(string content)
    {
        var records = new List<string[]>();
        var fields = new List<string>();
        bool inQuotes = false;
        var currentField = new System.Text.StringBuilder();

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < content.Length && content[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else if (!inQuotes && (c == '\n' || (c == '\r' && i + 1 < content.Length && content[i + 1] == '\n')))
            {
                if (c == '\r')
                    i++;
                fields.Add(currentField.ToString());
                currentField.Clear();

                if (fields.Count == 1 && string.IsNullOrEmpty(fields[0]))
                {
                    fields.Clear();
                    continue;
                }

                records.Add(fields.ToArray());
                fields.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        if (currentField.Length > 0 || fields.Count > 0)
        {
            fields.Add(currentField.ToString());
            if (!(fields.Count == 1 && string.IsNullOrEmpty(fields[0])))
            {
                records.Add(fields.ToArray());
            }
        }

        return records;
    }
}
