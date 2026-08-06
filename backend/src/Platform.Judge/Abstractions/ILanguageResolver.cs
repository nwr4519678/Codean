using Platform.Judge.Languages;

namespace Platform.Judge.Abstractions;

public interface ILanguageResolver
{
    bool IsSupported(string language);
    LanguageDefinition? Resolve(string language);
}
