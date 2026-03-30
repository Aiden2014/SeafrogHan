namespace SeafrogHan.Handler;

public class TmpMtextMissingCommonHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if (
            TranslationManager.TmpMTextMissingCommonTranslations.TryGetValue(
                ctx.NormalizedValue,
                out var translatedMissingCommonValue
            )
            || (
                ctx.TrimmedValue != ctx.NormalizedValue
                && TranslationManager.TmpMTextMissingCommonTranslations.TryGetValue(
                    ctx.TrimmedValue,
                    out translatedMissingCommonValue
                )
            )
        )
        {
            if (!string.IsNullOrEmpty(translatedMissingCommonValue))
            {
                return translatedMissingCommonValue;
            }
        }
        return base.Handle(ctx);
    }
}
