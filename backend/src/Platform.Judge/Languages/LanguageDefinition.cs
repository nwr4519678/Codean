namespace Platform.Judge.Languages;

public enum LanguageCategory
{
    Traditional,
    Browser
}

public record LanguageDefinition(
    string Code,
    string DisplayName,
    LanguageCategory Category,
    bool IsCompiled,
    bool IsInterpreted,
    string Extension
);
