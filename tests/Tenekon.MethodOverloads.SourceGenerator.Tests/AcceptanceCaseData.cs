using System.Collections;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class AcceptanceCaseData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var caseResult in AcceptanceFixtureCache.Instance.Cases) yield return new object[] { caseResult };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}