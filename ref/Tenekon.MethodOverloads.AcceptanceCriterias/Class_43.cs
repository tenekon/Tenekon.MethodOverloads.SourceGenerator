using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public static partial class Class_43_Bucket
{
}

public sealed class Class_43
{
    [GenerateOverloads(nameof(param_b))]
    [OverloadGenerationOptions(BucketType = typeof(Class_43_Bucket))]
    public void Case_1(int param_a, string? param_b)
    {
    }
}

public static class Class_43_AcceptanceCriterias
{
    public static void Case_1(this Class_43 source, int param_a)
    {
        source.Case_1(param_a, default(string));
    }
}
