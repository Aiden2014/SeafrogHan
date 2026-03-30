namespace SeafrogHan.Handler;

public class IgnoreTextHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if (
            MissingStringTracker.IsIgnoreMissingTmpMText(
                $"{ctx.GameObjectName}|||{ctx.OriginalValue}",
                ctx.OriginalValue
            )
        )
        {
            return ctx.OriginalValue;
        }
        return base.Handle(ctx);
    }
}
