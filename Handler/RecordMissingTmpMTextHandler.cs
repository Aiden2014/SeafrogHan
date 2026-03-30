namespace SeafrogHan.Handler;

public class RecordMissingTmpMTextHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        Plugin.Log.LogWarning(
            "No translation found for this text: " + ctx.OriginalValue + ", in GameObject: " + ctx.GameObjectName
        );
        MissingStringTracker.RecordMissingTmpMText($"{ctx.GameObjectName}|||{ctx.OriginalValue}", ctx.OriginalValue);
        return base.Handle(ctx);
    }
}
