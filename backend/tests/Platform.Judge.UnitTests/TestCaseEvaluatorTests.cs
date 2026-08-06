using System.Collections.Generic;
using FluentAssertions;
using Platform.Judge.Models;
using Platform.Judge.Services;
using Xunit;

namespace Platform.Judge.UnitTests;

public class TestCaseEvaluatorTests
{
    private readonly TestCaseEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_ShouldReturnAllPassed_WhenOutputMatchesExpected()
    {
        var testCases = new List<TestCase>
        {
            new("2 3", "5"),
            new("10 20", "30")
        };

        var (passedCount, results) = _evaluator.Evaluate(testCases, "5", null, 12.0, 1024);

        passedCount.Should().Be(1);
        results.Should().HaveCount(2);
        results[0].Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ShouldMaskOutputs_ForHiddenTestCases()
    {
        var testCases = new List<TestCase>
        {
            new("secret_input", "secret_expected", IsHidden: true)
        };

        var (_, results) = _evaluator.Evaluate(testCases, "different_output", null, 10.0, 1024);

        results[0].IsHidden.Should().BeTrue();
        results[0].ActualOutput.Should().Be("[HIDDEN]");
    }
}
