namespace SeafrogHan.Handler;

public class TmpMtextMissingHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if (
            TranslationManager.TmpMTextMissingTranslations.TryGetValue(
                $"{ctx.GameObjectName}",
                out var translatedMissing
            )
            && (
                translatedMissing.TryGetValue(ctx.NormalizedValue, out var translatedMissingValue)
                || (
                    ctx.TrimmedValue != ctx.NormalizedValue
                    && translatedMissing.TryGetValue(ctx.TrimmedValue, out translatedMissingValue)
                )
            )
        )
        {
            if (!string.IsNullOrEmpty(translatedMissingValue))
            {
                return translatedMissingValue;
            }
        }
        return base.Handle(ctx);
    }
}
