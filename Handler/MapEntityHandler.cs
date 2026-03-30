namespace SeafrogHan.Handler;

public class MapEntityHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if (
            "Header_text".Equals(ctx.GameObjectName)
            && TranslationManager.MapEntityDisplayNameTranslations.TryGetValue(
                ctx.NormalizedValue,
                out var translatedDisplayName
            )
        )
        {
            return translatedDisplayName;
        }
        if (
            "Description_text".Equals(ctx.GameObjectName)
            && TranslationManager.MapEntityDisplayInfoTranslations.TryGetValue(
                ctx.NormalizedValue,
                out var translatedDisplayInfo
            )
        )
        {
            return translatedDisplayInfo;
        }
        return base.Handle(ctx);
    }
}
