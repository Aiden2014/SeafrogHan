namespace SeafrogHan.Handler;

public class TmpMtextCommonHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if (
            (
                TranslationManager.TmpMTextCommonTranslations.TryGetValue(
                    ctx.NormalizedValue,
                    out var translatedCommonValue
                )
                || (
                    ctx.TrimmedValue != ctx.NormalizedValue
                    && TranslationManager.TmpMTextCommonTranslations.TryGetValue(
                        ctx.TrimmedValue,
                        out translatedCommonValue
                    )
                )
            )
            && translatedCommonValue != ctx.OriginalValue
        )
        {
            if (!string.IsNullOrEmpty(translatedCommonValue))
            {
                return translatedCommonValue;
            }
        }
        return base.Handle(ctx);
    }
}
