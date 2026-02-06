using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
internal interface Class_29_Matcher
{
    [GenerateOverloads(nameof(param_2))]
    void Matcher_1(Class_29_Target source, int param_1, string? param_2);

    [GenerateOverloads(nameof(text))]
    void Matcher_2(int value, string? text);
}

public sealed class Class_29_Target;

/// <summary>
/// Static extension methods mixed with non-extension static methods.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_29_Matcher)])]
public static class Class_29_Extensions
{
    public static void Case_1(this Class_29_Target source, int param_1, string? param_2) { }

    public static void Case_2(int value, string? text) { }
}

public static class Class_29_AcceptanceCriterias
{
    public static void Case_1(this Class_29_Target source, int param_1)
    {
        // ReSharper disable once InvokeAsExtensionMember
        Class_29_Extensions.Case_1(source, param_1, param_2: null);
    }

    extension(Class_29_Extensions)
    {
        public static void Case_2(int value)
        {
            Class_29_Extensions.Case_2(value, text: null);
        }
    }
}