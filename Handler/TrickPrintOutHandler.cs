namespace SeafrogHan.Handler;

public class TrickPrintOutHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if (ctx.GameObjectName.Equals("TrickPrintOut"))
        {
            Plugin.Log.LogInfo("Handling TrickPrintOut: " + ctx.NormalizedValue);
            var translatedTricks = TranslateTricksCombination(ctx.NormalizedValue);
            if (translatedTricks != ctx.NormalizedValue)
            {
                return translatedTricks;
            }
        }
        return base.Handle(ctx);
    }

    private static string TranslateTricksCombination(string value)
    {
        var trickTranslations = TranslationManager.TrickTranslations;
        if (string.IsNullOrEmpty(value) || trickTranslations == null)
        {
            return value;
        }
        value = System.Text.RegularExpressions.Regex.Replace(
            value,
            @"<color=([^>]*)>([^<]+)</color>",
            match =>
            {
                string colorCode = match.Groups[1].Value;
                string content = match.Groups[2].Value;

                string trickName = content.Trim();
                if (trickName.EndsWith("+"))
                {
                    trickName = trickName.Substring(0, trickName.Length - 1).Trim();
                }

                if (trickTranslations.TryGetValue(trickName, out var translation))
                {
                    string newContent = content.Replace(trickName, translation);
                    return $"<color={colorCode}>{newContent}</color>";
                }
                return match.Value;
            }
        );

        string[] lines = value.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = System.Text.RegularExpressions.Regex.Replace(
                lines[i],
                @"(?<=^|>)[^<]+(?=<|$)",
                match =>
                {
                    string textOutsideTags = match.Value;
                    string[] tricks = textOutsideTags.Split('+');
                    for (int j = 0; j < tricks.Length; j++)
                    {
                        string trimmed = tricks[j].Trim();
                        if (
                            !string.IsNullOrEmpty(trimmed)
                            && trickTranslations.TryGetValue(trimmed, out var translation)
                        )
                        {
                            tricks[j] = tricks[j].Replace(trimmed, translation);
                        }
                    }
                    return string.Join("+", tricks);
                }
            );
        }
        return string.Join("\n", lines);
    }
}
