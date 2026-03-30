namespace SeafrogHan.Handler;

public class TmpMtextHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if (
            TranslationManager.TmpMTextTranslations.TryGetValue($"{ctx.GameObjectName}", out var translated)
            && (
                translated.TryGetValue(ctx.NormalizedValue, out var translatedValue)
                || (
                    ctx.TrimmedValue != ctx.NormalizedValue
                    && translated.TryGetValue(ctx.TrimmedValue, out translatedValue)
                )
            )
            && translatedValue != ctx.OriginalValue
        )
        {
            if (!string.IsNullOrEmpty(translatedValue))
            {
                return translatedValue;
            }
        }
        return base.Handle(ctx);
    }
}
