using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SeafrogHan;

/// <summary>
/// 管理所有翻译字典和翻译查找逻辑
/// </summary>
public static class TranslationManager
{
    private static Dictionary<string, Dictionary<string, string>> dialogueMessageTranslationMap = [];
    private static Dictionary<string, Dictionary<string, string>> tmpMTextTranslationMap = [];
    private static Dictionary<string, string> tmpMTextCommonTranslationMap = [];
    private static Dictionary<string, Dictionary<string, string>> tmpMTextMissingTranslationMap = [];
    private static Dictionary<string, string> tmpMTextMissingCommonTranslationMap = [];
    private static Dictionary<string, string> trickTranslationMap = [];
    private static Dictionary<string, string> mapEntityDisplayNameTranslationMap = [];
    private static Dictionary<string, string> mapEntityDisplayInfoTranslationMap = [];

    public static Dictionary<string, Dictionary<string, string>> DialogueMessageTranslations =>
        dialogueMessageTranslationMap;
    public static Dictionary<string, Dictionary<string, string>> TmpMTextTranslations => tmpMTextTranslationMap;
    public static Dictionary<string, string> TmpMTextCommonTranslations => tmpMTextCommonTranslationMap;
    public static Dictionary<string, Dictionary<string, string>> TmpMTextMissingTranslations =>
        tmpMTextMissingTranslationMap;
    public static Dictionary<string, string> TmpMTextMissingCommonTranslations => tmpMTextMissingCommonTranslationMap;
    public static Dictionary<string, string> TrickTranslations => trickTranslationMap;
    public static Dictionary<string, string> MapEntityDisplayNameTranslations => mapEntityDisplayNameTranslationMap;
    public static Dictionary<string, string> MapEntityDisplayInfoTranslations => mapEntityDisplayInfoTranslationMap;

    private static bool isInitialized = false;

    public static string unmatchTricksAndBoostDescription;

    public static void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        try
        {
            Plugin.Log.LogInfo("Initializing translations...");

            dialogueMessageTranslationMap = ResourceLoader.GetDialogueMessageTranslations("dialogue_message.csv");
            tmpMTextTranslationMap = ResourceLoader.GetTmpMTextTranslations("tmp_m_text.csv");
            tmpMTextCommonTranslationMap = ResourceLoader.GetTmpMTextCommonTranslations("tmp_m_text.csv");
            tmpMTextMissingTranslationMap = ResourceLoader.GetTmpMTextTranslations("tmp_m_text_missing.csv");
            tmpMTextMissingCommonTranslationMap = ResourceLoader.GetTmpMTextCommonTranslations(
                "tmp_m_text_missing.csv"
            );
            trickTranslationMap = ResourceLoader.GetTmpMTextCommonTranslations("trick.csv");
            mapEntityDisplayNameTranslationMap = ResourceLoader.GetTmpMTextCommonTranslations(
                "map_entity_display_name.csv"
            );
            mapEntityDisplayInfoTranslationMap = ResourceLoader.GetTmpMTextCommonTranslations(
                "map_entity_display_info.csv"
            );
            isInitialized = true;
            Plugin.Log.LogInfo("Translations initialized successfully");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to initialize translations: {ex.Message}");
            isInitialized = true; // 避免重复尝试
        }
    }
}
