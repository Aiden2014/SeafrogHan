using System.Collections.Generic;
using System.IO;
using Il2CppInterop.Runtime;
using TMPro;
using UnityEngine;

namespace SeafrogHan;

public static class ResourceLoader
{
    private static string translationFilePath = "resources";
    private static readonly Dictionary<string, TMP_FontAsset> _cachedChineseFonts =
        new Dictionary<string, TMP_FontAsset>();

    public static TMP_FontAsset LoadChineseFont(string bundleName, string fontAssetName)
    {
        string cacheKey = $"{bundleName}|||{fontAssetName}";
        if (_cachedChineseFonts.TryGetValue(cacheKey, out var cachedFont) && cachedFont != null)
        {
            return cachedFont;
        }

        var bundlePath = System.IO.Path.Combine(BepInEx.Paths.PluginPath, translationFilePath, bundleName);

        Plugin.Log.LogInfo($"Loading font bundle from: {bundlePath}");

        if (!File.Exists(bundlePath))
        {
            Plugin.Log.LogError($"Font bundle file not found: {bundlePath}");
            return null;
        }

        var fontBundle = AssetBundle.LoadFromFile(bundlePath);
        if (fontBundle == null)
        {
            Plugin.Log.LogError($"Failed to load font asset bundle: {bundleName}");
            return null;
        }

        // List all asset names for debugging
        var allNames = fontBundle.GetAllAssetNames();
        foreach (var n in allNames)
            Plugin.Log.LogInfo($"  Bundle contains asset: {n}");

        // Use non-generic LoadAsset to avoid IL2CPP generic cast issues
        var loaded = fontBundle.LoadAsset(fontAssetName, Il2CppType.Of<TMP_FontAsset>());
        if (loaded == null)
        {
            // Try alternative names
            loaded = fontBundle.LoadAsset("KN Maiyuan", Il2CppType.Of<TMP_FontAsset>());
        }
        if (loaded == null)
        {
            Plugin.Log.LogError($"Failed to load {fontAssetName} from bundle!");
            return null;
        }

        var chineseFont = loaded.Cast<TMP_FontAsset>();
        _cachedChineseFonts[cacheKey] = chineseFont;
        Plugin.Log.LogInfo($"Successfully loaded {fontAssetName} font");
        return chineseFont;
    }

    private static HashSet<string> DialogueMessageSet = new HashSet<string>();

    public static bool IsDialogueMessage(string message)
    {
        string normalized = CsvHelper.NormalizeLineEndingsToCrlf(message);
        return DialogueMessageSet.Contains(normalized)
            || DialogueMessageSet.Contains(CsvHelper.TrimTrailingLineEndings(normalized));
    }

    public static Dictionary<string, Dictionary<string, string>> GetDialogueMessageTranslations(string csvFileName)
    {
        var translationMap = new Dictionary<string, Dictionary<string, string>>();

        try
        {
            string pluginDir = Path.GetDirectoryName(typeof(ResourceLoader).Assembly.Location);
            string translationPath = Path.Combine(pluginDir, translationFilePath, csvFileName);
            if (File.Exists(translationPath))
            {
                string[] lines = File.ReadAllLines(translationPath, System.Text.Encoding.UTF8);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parseResult = CsvHelper.ParseCsvRecords(line);
                    if (parseResult.Count == 0)
                        continue;

                    string[] parts = parseResult[0];
                    if (parts.Length >= 3)
                    {
                        // 第一列格式: bundle|||path_id|||m_Name|||message
                        // 提取第二群到第三群竖线之间的值作为外层key
                        string firstCol = parts[0];
                        string dialogueMessageKey = ExtractDialogueMessageKey(firstCol);

                        // 第二列作为内层key，第三列作为value
                        string innerKey = CsvHelper.NormalizeLineEndingsToCrlf(parts[1]);
                        string value = CsvHelper.NormalizeLineEndingsToCrlf(parts[2]);

                        if (string.IsNullOrEmpty(innerKey))
                            continue;

                        if (!translationMap.ContainsKey(dialogueMessageKey))
                        {
                            translationMap[dialogueMessageKey] = new Dictionary<string, string>();
                        }

                        if (!translationMap[dialogueMessageKey].ContainsKey(innerKey))
                        {
                            translationMap[dialogueMessageKey][innerKey] = value;
                            DialogueMessageSet.Add(innerKey);
                        }
                    }
                }
            }
            else
            {
                SeafrogHan.Plugin.Log.LogWarning(
                    $"[GetDialogueMessageTranslations] Translation file does not exist: {translationPath}"
                );
            }
        }
        catch (System.Exception ex)
        {
            SeafrogHan.Plugin.Log.LogError(
                $"[GetDialogueMessageTranslations] Failed to load translation file: {ex.Message}"
            );
        }

        return translationMap;
    }

    public static Dictionary<string, Dictionary<string, string>> GetTmpMTextTranslations(string csvFileName)
    {
        var translationMap = new Dictionary<string, Dictionary<string, string>>();

        try
        {
            string pluginDir = Path.GetDirectoryName(typeof(ResourceLoader).Assembly.Location);
            string translationPath = Path.Combine(pluginDir, translationFilePath, csvFileName);
            if (File.Exists(translationPath))
            {
                var records = CsvHelper.ParseCsvRecords(File.ReadAllText(translationPath, System.Text.Encoding.UTF8));
                foreach (var fields in records)
                {
                    if (fields.Length >= 2)
                    {
                        // 第一列格式: hierarchy_path|||m_text
                        // 提取到竖线之间的内容作为外层key
                        string firstCol = CsvHelper.NormalizeLineEndingsToCrlf(fields[0]);
                        string dialogueMessageKey = ExtractTmpMTextKey(firstCol);

                        // 优先使用第一列中 ||| 后面的文本作为内层key
                        string innerKey = ExtractTmpMTextInnerValue(firstCol);
                        if (string.IsNullOrEmpty(innerKey))
                        {
                            innerKey = CsvHelper.NormalizeLineEndingsToCrlf(fields[1]);
                        }

                        // value 兼容两种格式：
                        // 1) col2 为译文（常见于 tmp_m_text.csv）
                        // 2) col3 为译文（历史格式）
                        string value =
                            fields.Length >= 3 && !string.IsNullOrEmpty(fields[2])
                                ? CsvHelper.NormalizeLineEndingsToCrlf(fields[2])
                                : CsvHelper.NormalizeLineEndingsToCrlf(fields[1]);

                        if (string.IsNullOrEmpty(innerKey))
                            continue;

                        if (!translationMap.ContainsKey(dialogueMessageKey))
                        {
                            translationMap[dialogueMessageKey] = new Dictionary<string, string>();
                        }

                        // 同时存储原始key和去除末尾换行的key，确保多行文本能匹配
                        if (!translationMap[dialogueMessageKey].ContainsKey(innerKey))
                        {
                            translationMap[dialogueMessageKey][innerKey] = value;
                        }
                        string trimmedInnerKey = CsvHelper.TrimTrailingLineEndings(innerKey);
                        if (
                            trimmedInnerKey != innerKey
                            && !translationMap[dialogueMessageKey].ContainsKey(trimmedInnerKey)
                        )
                        {
                            translationMap[dialogueMessageKey][trimmedInnerKey] = value;
                        }
                    }
                }
            }

            // 调试：打印 Description text 下的所有 key
            if (translationMap.ContainsKey("Description text"))
            {
                SeafrogHan.Plugin.Log.LogInfo(
                    $"[GetTmpMTextTranslations][{csvFileName}] 'Description text' has {translationMap["Description text"].Count} entries:"
                );
                foreach (var kv in translationMap["Description text"])
                {
                    string escapedKey = kv.Key.Replace("\r", "\\r").Replace("\n", "\\n");
                    string escapedVal = (kv.Value ?? "").Replace("\r", "\\r").Replace("\n", "\\n");
                    SeafrogHan.Plugin.Log.LogInfo(
                        $"  KEY=[{escapedKey}] VAL=[{escapedVal.Substring(0, System.Math.Min(60, escapedVal.Length))}]"
                    );
                }
            }
            else
            {
                SeafrogHan.Plugin.Log.LogWarning(
                    $"[GetTmpMTextTranslations] Translation file does not exist: {translationPath}"
                );
            }
        }
        catch (System.Exception ex)
        {
            SeafrogHan.Plugin.Log.LogError($"[GetTmpMTextTranslations] Failed to load translation file: {ex.Message}");
        }

        return translationMap;
    }

    public static Dictionary<string, string> GetTmpMTextCommonTranslations(string csvFileName)
    {
        Dictionary<string, string> translationMap = new Dictionary<string, string>();

        try
        {
            // 从plugin资源目录读取翻译文件
            string pluginDir = Path.GetDirectoryName(typeof(ResourceLoader).Assembly.Location);
            string translationPath = Path.Combine(pluginDir, translationFilePath, csvFileName);
            if (File.Exists(translationPath))
            {
                var records = CsvHelper.ParseCsvRecords(File.ReadAllText(translationPath, System.Text.Encoding.UTF8));
                foreach (var fields in records)
                {
                    if (fields.Length >= 2)
                    {
                        // 通用映射按”原文 -> 译文”组织
                        string firstCol = CsvHelper.NormalizeLineEndingsToCrlf(fields[0]);
                        string key = ExtractTmpMTextInnerValue(firstCol);
                        if (string.IsNullOrEmpty(key))
                        {
                            key = CsvHelper.NormalizeLineEndingsToCrlf(fields[1]);
                        }

                        string value =
                            fields.Length >= 3 && !string.IsNullOrEmpty(fields[2])
                                ? CsvHelper.NormalizeLineEndingsToCrlf(fields[2])
                                : CsvHelper.NormalizeLineEndingsToCrlf(fields[1]);

                        if (string.IsNullOrEmpty(key))
                            continue;

                        // 同时存储原始key和去除末尾换行的key，确保多行文本能匹配
                        if (!translationMap.ContainsKey(key))
                        {
                            translationMap[key] = value;
                        }
                        string trimmedKey = CsvHelper.TrimTrailingLineEndings(key);
                        if (trimmedKey != key && !translationMap.ContainsKey(trimmedKey))
                        {
                            translationMap[trimmedKey] = value;
                        }
                        if (
                            string.IsNullOrEmpty(TranslationManager.unmatchTricksAndBoostDescription)
                            && key.StartsWith("<color=#B07600>Release")
                        )
                        {
                            TranslationManager.unmatchTricksAndBoostDescription = value;
                        }
                    }
                }
            }
            else
            {
                SeafrogHan.Plugin.Log.LogWarning(
                    $"[GetTmpMTextCommonTranslations] Translation file does not exist: {translationPath}"
                );
            }
        }
        catch (System.Exception ex)
        {
            SeafrogHan.Plugin.Log.LogError(
                $"[GetTmpMTextCommonTranslations] Failed to load translation file: {ex.Message}"
            );
        }

        return translationMap;
    }

    private static string ExtractDialogueMessageKey(string input)
    {
        // 格式: bundle|||path_id|||m_Name|||message
        // 需要提取第二群竖线到第三群竖线之间的内容: m_Name
        if (string.IsNullOrEmpty(input))
            return null;

        int firstDash = input.IndexOf("|||");
        if (firstDash < 0)
            return null;

        int secondDash = input.IndexOf("|||", firstDash + 3);
        if (secondDash < 0)
            return null;

        int thirdDash = input.IndexOf("|||", secondDash + 3);
        if (thirdDash < 0)
            return null;

        return input.Substring(secondDash + 3, thirdDash - secondDash - 3);
    }

    private static string ExtractTmpMTextKey(string input)
    {
        // 格式: hierarchy_path|||m_text
        // 需要提取到竖线之间的内容: hierarchy_path
        if (string.IsNullOrEmpty(input))
            return null;

        int firstDash = input.IndexOf("|||");
        if (firstDash < 0)
            return null;
        return input.Substring(0, firstDash);
    }

    private static string ExtractTmpMTextInnerValue(string input)
    {
        // 格式: hierarchy_path|||m_text
        // 需要提取竖线后的内容: m_text
        if (string.IsNullOrEmpty(input))
            return null;

        int firstDash = input.IndexOf("|||");
        if (firstDash < 0)
            return null;
        return input.Substring(firstDash + 3);
    }
}
