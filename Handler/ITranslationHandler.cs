namespace SeafrogHan.Handler;

public interface ITranslationHandler
{
    ITranslationHandler SetNext(ITranslationHandler handler);
    string Handle(TranslationContext ctx);
}
