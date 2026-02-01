using System.Collections;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class AcceptanceDiagnosticsData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var caseResult in AcceptanceFixtureCache.Instance.DiagnosticCases)
        {
            yield return new object[] { caseResult };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
