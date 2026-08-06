using FluentAssertions;
using Platform.Judge.Languages;
using Xunit;

namespace Platform.Judge.UnitTests;

public class LanguageResolverTests
{
    private readonly LanguageResolver _resolver = new();

    [Theory]
    [InlineData("csharp", "C#")]
    [InlineData("cs", "C#")]
    [InlineData("python", "Python")]
    [InlineData("py", "Python")]
    [InlineData("cpp", "C++")]
    [InlineData("java", "Java")]
    [InlineData("html", "HTML")]
    public void IsSupported_ShouldReturnTrue_ForKnownLanguages(string input, string expectedDisplayName)
    {
        var isSupported = _resolver.IsSupported(input);
        var resolved = _resolver.Resolve(input);

        isSupported.Should().BeTrue();
        resolved.Should().NotBeNull();
        resolved!.DisplayName.Should().Be(expectedDisplayName);
    }

    [Theory]
    [InlineData("unknown_lang")]
    [InlineData("")]
    [InlineData(null)]
    public void Resolve_ShouldReturnNull_ForUnsupportedLanguages(string? input)
    {
        var resolved = _resolver.Resolve(input!);
        resolved.Should().BeNull();
    }
}
