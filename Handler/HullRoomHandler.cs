namespace SeafrogHan.Handler;

public class HullRoomHandler : TranslationHandlerBase
{
    public override string Handle(TranslationContext ctx)
    {
        if ("Description_text".Equals(ctx.GameObjectName))
        {
            var hullMatch = System.Text.RegularExpressions.Regex.Match(
                ctx.NormalizedValue,
                @"^(?:Hull )([A-Z]) Room (.+)$"
            );
            if (hullMatch.Success)
            {
                var isDoubleHull = hullMatch.Groups[2].Value.Equals("Hull " + hullMatch.Groups[1].Value);
                if (isDoubleHull)
                {
                    return hullMatch.Groups[1].Value + " 号舱 ";
                }
                if (hullMatch.Groups[2].Value.Equals("Engine"))
                {
                    return $"{hullMatch.Groups[1].Value} 号舱引擎室";
                }
                return $"{hullMatch.Groups[1].Value} 号舱 {hullMatch.Groups[2].Value} 室";
            }
        }
        return base.Handle(ctx);
    }
}
