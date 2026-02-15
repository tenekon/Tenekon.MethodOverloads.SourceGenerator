using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public sealed class Class_44_Bucket
{
}

[SuppressMessage("MethodOverloadsGenerator", "MOG013")]
public sealed class Class_44
{
    [GenerateOverloads(nameof(param_b))]
    [OverloadGenerationOptions(BucketType = typeof(Class_44_Bucket))]
    public void Case_1(int param_a, string? param_b)
    {
    }
}
