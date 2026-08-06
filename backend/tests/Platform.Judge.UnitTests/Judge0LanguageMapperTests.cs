using FluentAssertions;
using Platform.Judge.Engines.Judge0;
using Xunit;

namespace Platform.Judge.UnitTests;

public class Judge0LanguageMapperTests
{
    [Theory]
    [InlineData("csharp", 51)]
    [InlineData("python", 71)]
    [InlineData("java", 62)]
    [InlineData("cpp", 54)]
    [InlineData("javascript", 63)]
    public void GetLanguageId_ShouldMapStandardLanguages_ToJudge0Ids(string language, int expectedId)
    {
        var id = Judge0LanguageMapper.GetLanguageId(language);
        id.Should().Be(expectedId);
    }

    [Fact]
    public void GetLanguageId_ShouldReturnNull_ForUnknownLanguage()
    {
        var id = Judge0LanguageMapper.GetLanguageId("cobol");
        id.Should().BeNull();
    }
}
