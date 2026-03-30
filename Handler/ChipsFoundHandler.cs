using System.Text.RegularExpressions;

namespace SeafrogHan.Handler;

public class ChipsFoundHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        Regex pattern = new Regex(@"^\d+/\d+ P\.O\.T\.A\.T\.O Chips Found$");
        if (pattern.IsMatch(ctx.NormalizedValue))
        {
            var numbers = ctx.NormalizedValue.Split(' ')[0].Split('/');
            return "已找到 " + numbers[0] + "/" + numbers[1] + " 个马.铃.薯.芯片";
        }
        return base.Handle(ctx);
    }
}
