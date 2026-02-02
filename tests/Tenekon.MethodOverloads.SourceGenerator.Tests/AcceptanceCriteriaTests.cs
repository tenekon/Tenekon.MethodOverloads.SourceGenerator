namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class AcceptanceCriteriaTests
{
    [Fact]
    public void Generated_attributes_are_present()
    {
        var fixture = AcceptanceFixtureCache.Instance;
        Assert.True(fixture.HasGenerateOverloadsAttribute);
        Assert.True(fixture.HasGenerateMethodOverloadsAttribute);
    }

    [Theory]
    [ClassData(typeof(AcceptanceCaseData))]
    public void Generated_overloads_match_acceptance_criteria(CaseResult caseResult)
    {
        var expectedKeys = caseResult.ExpectedKeys;
        var actualKeys = caseResult.ActualKeys;

        if (!expectedKeys.SetEquals(actualKeys))
        {
            var missing = expectedKeys.Except(actualKeys).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            var extra = actualKeys.Except(expectedKeys).OrderBy(x => x, StringComparer.Ordinal).ToArray();

            var message = "[" + caseResult.ClassName + "]\nMissing:\n" + string.Join("\n", missing) + "\n\nExtra:\n" + string.Join("\n", extra);
            Assert.Fail(message);
        }
    }
}
