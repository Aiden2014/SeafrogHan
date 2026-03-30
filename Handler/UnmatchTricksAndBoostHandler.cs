namespace SeafrogHan.Handler;

public class UnmatchTricksAndBoostHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if ("Description text".Equals(ctx.GameObjectName) && ctx.OriginalValue.StartsWith("<color=#B07600>Release"))
        {
            return TranslationManager.unmatchTricksAndBoostDescription;
        }
        return base.Handle(ctx);
    }
}
