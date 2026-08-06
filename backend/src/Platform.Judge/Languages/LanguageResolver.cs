using System;
using System.Collections.Generic;
using Platform.Judge.Abstractions;

namespace Platform.Judge.Languages;

public class LanguageResolver : ILanguageResolver
{
    private static readonly Dictionary<string, LanguageDefinition> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp"] = new LanguageDefinition("csharp", "C#", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".cs"),
        ["cs"] = new LanguageDefinition("csharp", "C#", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".cs"),
        ["java"] = new LanguageDefinition("java", "Java", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".java"),
        ["python"] = new LanguageDefinition("python", "Python", LanguageCategory.Traditional, IsCompiled: false, IsInterpreted: true, Extension: ".py"),
        ["py"] = new LanguageDefinition("python", "Python", LanguageCategory.Traditional, IsCompiled: false, IsInterpreted: true, Extension: ".py"),
        ["javascript"] = new LanguageDefinition("javascript", "JavaScript (Node.js)", LanguageCategory.Traditional, IsCompiled: false, IsInterpreted: true, Extension: ".js"),
        ["js"] = new LanguageDefinition("javascript", "JavaScript (Node.js)", LanguageCategory.Traditional, IsCompiled: false, IsInterpreted: true, Extension: ".js"),
        ["cpp"] = new LanguageDefinition("cpp", "C++", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".cpp"),
        ["c"] = new LanguageDefinition("c", "C", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".c"),
        ["go"] = new LanguageDefinition("go", "Go", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".go"),
        ["rust"] = new LanguageDefinition("rust", "Rust", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".rs"),
        ["kotlin"] = new LanguageDefinition("kotlin", "Kotlin", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".kt"),
        ["pascal"] = new LanguageDefinition("pascal", "Pascal", LanguageCategory.Traditional, IsCompiled: true, IsInterpreted: false, Extension: ".pas"),
        ["html"] = new LanguageDefinition("html", "HTML", LanguageCategory.Browser, IsCompiled: false, IsInterpreted: true, Extension: ".html"),
        ["css"] = new LanguageDefinition("css", "CSS", LanguageCategory.Browser, IsCompiled: false, IsInterpreted: true, Extension: ".css"),
        ["browserjs"] = new LanguageDefinition("browserjs", "Browser JavaScript", LanguageCategory.Browser, IsCompiled: false, IsInterpreted: true, Extension: ".js")
    };

    public bool IsSupported(string language)
    {
        return !string.IsNullOrWhiteSpace(language) && SupportedLanguages.ContainsKey(language);
    }

    public LanguageDefinition? Resolve(string language)
    {
        if (string.IsNullOrWhiteSpace(language)) return null;
        return SupportedLanguages.TryGetValue(language, out var def) ? def : null;
    }
}
