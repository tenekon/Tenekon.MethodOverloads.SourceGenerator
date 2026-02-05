using Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class AcceptanceDiagnosticsTests
{
    [Theory]
    [ClassData(typeof(AcceptanceDiagnosticsData))]
    public void Expected_diagnostics_are_reported(DiagnosticCaseResult caseResult)
    {
        if (caseResult.ExpectedIds.Count == 0)
        {
            if (caseResult.ActualIds.Count == 0) return;

            var unexpected = caseResult.ActualIds.OrderBy(x => x, StringComparer.Ordinal).ToArray();
            var unexpectedMessage = "[" + caseResult.ClassName + "]\nUnexpected diagnostics:\n"
                + string.Join("\n", unexpected);
            Assert.Fail(unexpectedMessage);
        }

        var missing = caseResult.ExpectedIds.Except(caseResult.ActualIds)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        var extra = caseResult.ActualIds.Except(caseResult.ExpectedIds)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        if (missing.Length == 0 && extra.Length == 0) return;

        var message = "[" + caseResult.ClassName + "]\nMissing diagnostics:\n" + string.Join("\n", missing)
            + "\n\nUnexpected diagnostics:\n" + string.Join("\n", extra);
        Assert.Fail(message);
    }
}