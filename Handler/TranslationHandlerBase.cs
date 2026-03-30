namespace SeafrogHan.Handler;

public abstract class TranslationHandlerBase : ITranslationHandler
{
    private ITranslationHandler _nextTranslationHandler;

    public ITranslationHandler SetNext(ITranslationHandler nextHandler)
    {
        _nextTranslationHandler = nextHandler;
        return nextHandler;
    }

    public virtual string Handle(TranslationContext ctx)
    {
        if (_nextTranslationHandler != null)
        {
            return _nextTranslationHandler.Handle(ctx);
        }
        return ctx.OriginalValue;
    }

    public static ITranslationHandler LinkHandlers(params ITranslationHandler[] handlers)
    {
        if (handlers == null || handlers.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < handlers.Length - 1; i++)
        {
            handlers[i].SetNext(handlers[i + 1]);
        }
        return handlers[0];
    }
}
