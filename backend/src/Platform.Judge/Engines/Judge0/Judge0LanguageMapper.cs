using System;
using System.Collections.Generic;

namespace Platform.Judge.Engines.Judge0;

public class Judge0LanguageMapper
{
    private static readonly Dictionary<string, int> LanguageToJudge0IdMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp"] = 51,
        ["cs"] = 51,
        ["java"] = 62,
        ["python"] = 71,
        ["py"] = 71,
        ["javascript"] = 63,
        ["js"] = 63,
        ["cpp"] = 54,
        ["c"] = 50,
        ["go"] = 60,
        ["rust"] = 73,
        ["kotlin"] = 78,
        ["pascal"] = 67
    };

    public static int? GetLanguageId(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return null;
        return LanguageToJudge0IdMap.TryGetValue(languageCode, out var id) ? id : null;
    }
}
