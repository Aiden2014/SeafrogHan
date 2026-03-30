namespace SeafrogHan.Handler;

public class TranslationContext(
    string gameObjectName,
    string originalValue,
    string normalizedValue,
    string trimmedValue
)
{
    public string GameObjectName { get; set; } = gameObjectName;
    public string OriginalValue { get; set; } = originalValue;
    public string NormalizedValue { get; set; } = normalizedValue;
    public string TrimmedValue { get; set; } = trimmedValue;
}
