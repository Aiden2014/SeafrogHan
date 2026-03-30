using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SeafrogHan.Handler;
using TMPro;
using UnityEngine;

namespace SeafrogHan;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        TranslationManager.Initialize();
        MissingStringTracker.Initialize();
        Harmony.CreateAndPatchAll(typeof(Hooks));
        AddGlobalFallback(ResourceLoader.LoadChineseFont("chinese_font.bundle", "KNMaiyuan"));
    }

    public static void AddGlobalFallback(TMP_FontAsset chineseFont)
    {
        var globalFallbacks = TMP_Settings.fallbackFontAssets;
        if (globalFallbacks != null && !globalFallbacks.Contains(chineseFont))
        {
            globalFallbacks.Add(chineseFont);
        }
    }
}

public static class Hooks
{
    public static void ReplaceFont(TMP_FontAsset __instance, TMP_FontAsset chineseFont)
    {
        __instance.atlasTextures = chineseFont.atlasTextures;
        __instance.atlasWidth = chineseFont.atlasWidth;
        __instance.atlasHeight = chineseFont.atlasHeight;
        __instance.atlasPadding = chineseFont.atlasPadding;

        __instance.faceInfo = chineseFont.faceInfo;
        __instance.glyphTable = chineseFont.glyphTable;
        __instance.characterTable = chineseFont.characterTable;

        __instance.material.mainTexture = chineseFont.atlasTextures[0];

        __instance.fontWeights = chineseFont.fontWeights;

        if (chineseFont.fallbackFontAssetTable != null)
        {
            __instance.fallbackFontAssetTable = chineseFont.fallbackFontAssetTable;
        }
    }

    private static Dictionary<string, (string bundleName, string fontName)> fallbackConfigs = new Dictionary<
        string,
        (string, string)
    >
    {
        { "Grandstander-Bold", ("chinese_font.bundle", "KNMaiyuan") },
        {
            "Grandstander-Bold BlueWhiteStroke",
            ("chinese_font_blue_white_stroke.bundle", "KN Maiyuan BlueWhiteStroke")
        },
        { "Grandstander-Bold WhiteStroke", ("chinese_font_white_stroke.bundle", "KN Maiyuan WhiteStroke") },
        { "Daydream SDF", ("chinese_font_daydream.bundle", "AaHuanMengKongJianXiangSuTi SDF") },
        { "upheavtt SDF", ("chinese_font_upheavtt.bundle", "ZhanKuKuHei SDF") },
    };

    private static Dictionary<string, TMP_FontAsset> loadedFallbacks = new Dictionary<string, TMP_FontAsset>();

    [HarmonyPatch(typeof(TMP_FontAsset), nameof(TMP_FontAsset.ReadFontAssetDefinition))]
    [HarmonyPostfix]
    public static void TMP_FontAsset_ReadFontAssetDefinition_Postfix(TMP_FontAsset __instance)
    {
        if (__instance == null || __instance.name == null)
            return;

        Plugin.Log.LogInfo($"Finished reading font asset definition for: {__instance.name}");

        if (fallbackConfigs.TryGetValue(__instance.name, out var config))
        {
            if (HasFallback(__instance, config.fontName))
                return;

            // 如果还没加载过这个字体，再调用 ResourceLoader 去加载它
            if (!loadedFallbacks.TryGetValue(__instance.name, out var chineseFallbackFont))
            {
                chineseFallbackFont = ResourceLoader.LoadChineseFont(config.bundleName, config.fontName);
                loadedFallbacks[__instance.name] = chineseFallbackFont;
            }

            if (chineseFallbackFont == null)
            {
                return;
            }
            Plugin.Log.LogInfo($"Saving fallback for font: {chineseFallbackFont.name} in {__instance.name}");
            SaveFallbackFont(__instance, chineseFallbackFont);
        }
    }

    private static bool HasFallback(TMP_FontAsset font, string fallbackName)
    {
        if (font?.fallbackFontAssetTable == null)
            return false;

        for (int i = 0; i < font.fallbackFontAssetTable.Count; i++)
        {
            var existing = font.fallbackFontAssetTable[i];
            if (existing != null && existing.name == fallbackName)
            {
                return true;
            }
        }

        return false;
    }

    private static void SaveFallbackFont(TMP_FontAsset __instance, TMP_FontAsset chineseFallbackFont)
    {
        if (__instance.fallbackFontAssetTable == null)
        {
            __instance.fallbackFontAssetTable = new Il2CppSystem.Collections.Generic.List<TMP_FontAsset>();
        }

        bool alreadyAdded = false;
        for (int i = 0; i < __instance.fallbackFontAssetTable.Count; i++)
        {
            if (__instance.fallbackFontAssetTable[i].name == chineseFallbackFont.name)
            {
                alreadyAdded = true;
                break;
            }
        }

        if (!alreadyAdded)
        {
            __instance.fallbackFontAssetTable.Add(chineseFallbackFont);
        }
    }

    [HarmonyPatch(typeof(CaptainDialogueBubble), nameof(CaptainDialogueBubble.PlayConversation))]
    [HarmonyPostfix]
    public static void CaptainDialogueBubble_PlayConversation_Prefix(CaptainDialogueBubble __instance)
    {
        if (
            TranslationManager.DialogueMessageTranslations.TryGetValue(
                __instance.CurrentConversation.name,
                out var messageMap
            )
        )
        {
            foreach (var entry in __instance.CurrentConversation.entries)
            {
                if (
                    messageMap.TryGetValue(entry.message, out var translatedMessage)
                    && !string.IsNullOrEmpty(translatedMessage)
                )
                {
                    entry.message = translatedMessage;
                }
            }
        }
    }

    static readonly ITranslationHandler setTextHandler = TranslationHandlerBase.LinkHandlers(
        new TrickPrintOutHandler(),
        new ChipsFoundHandler(),
        new HullRoomHandler(),
        new MapEntityHandler(),
        new UnmatchTricksAndBoostHandler(),
        new IgnoreTextHandler(),
        new TmpMtextHandler(),
        new TmpMtextCommonHandler(),
        new TmpMtextMissingHandler(),
        new TmpMtextMissingCommonHandler(),
        new RecordMissingTmpMTextHandler()
    );

    [HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.text), MethodType.Setter)]
    [HarmonyPrefix]
    public static void TMP_Text_SetText_Prefix(TMP_Text __instance, ref string value)
    {
        string goName = __instance.gameObject.name;
        string normalizedValue = CsvHelper.NormalizeLineEndingsToCrlf(value);
        string trimmedValue = CsvHelper.TrimTrailingLineEndings(normalizedValue);
        var context = new TranslationContext(goName, value, normalizedValue, trimmedValue);

        if (__instance.text.Equals("PERFECT JUMP!"))
        {
            __instance.lineSpacing = 5f;
            __instance.rectTransform.sizeDelta = new Vector2(200f, __instance.rectTransform.sizeDelta.y);
        }

        value = setTextHandler.Handle(context);
    }

    static readonly ITranslationHandler TMPAwakeHandler = TranslationHandlerBase.LinkHandlers(
        new IgnoreTextHandler(),
        new TmpMtextHandler(),
        new TmpMtextCommonHandler(),
        new TmpMtextMissingHandler(),
        new TmpMtextMissingCommonHandler(),
        new RecordMissingTmpMTextHandler()
    );

    [HarmonyPatch(typeof(TextMeshProUGUI), nameof(TextMeshProUGUI.Awake))]
    [HarmonyPostfix]
    public static void TMP_Text_Awake_Prefix(TextMeshProUGUI __instance)
    {
        string goName = __instance.gameObject.name;
        string normalizedValue = CsvHelper.NormalizeLineEndingsToCrlf(__instance.text);
        string trimmedValue = CsvHelper.TrimTrailingLineEndings(normalizedValue);
        TranslationContext context = new TranslationContext(goName, __instance.text, normalizedValue, trimmedValue);
        __instance.text = TMPAwakeHandler.Handle(context);
    }

    [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.Awake))]
    [HarmonyPrefix]
    public static void TextMeshPro_Awake_Prefix(TextMeshPro __instance)
    {
        string goName = __instance.gameObject.name;
        string normalizedValue = CsvHelper.NormalizeLineEndingsToCrlf(__instance.text);
        string trimmedValue = CsvHelper.TrimTrailingLineEndings(normalizedValue);

        var context = new TranslationContext(goName, __instance.text, normalizedValue, trimmedValue);

        if (
            __instance.text.Equals("Return to Hammerhead?")
            && loadedFallbacks.TryGetValue(__instance.font.name, out var fallbackFont)
        )
        {
            ReplaceFont(__instance.font, fallbackFont);
        }

        __instance.text = TMPAwakeHandler.Handle(context);
    }
}
