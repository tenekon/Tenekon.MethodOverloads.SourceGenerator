namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Static vs instance targets with C# 14 extension blocks.
/// Expects overloads for both static and non-static targets.
/// </summary>
public static class Class_14
{
    [GenerateOverloads(nameof(param_2))]
    public static void Case_1(int param_1, string? param_2) { }
}

/// <summary>
/// Instance target with classic overload generation behavior.
/// </summary>
public abstract class Class_14_Instance
{
    [GenerateOverloads(nameof(param_2))]
    public abstract void Case_1(int param_1, string? param_2);
}

/// <summary>
/// Expected extension overloads for Class_14 and Class_14_Instance cases.
/// </summary>
public static class Class_14_AcceptanceCriterias
{
    public static void Case_1(this Class_14_Instance source, int param_1)
    {
        source.Case_1(param_1, param_2: null);
    }
}

/// <summary>
/// Expected extension block for static target overloads (C# 14 extension members).
/// </summary>
public static class Class_14_Static_AcceptanceCriterias
{
    extension(Class_14)
    {
        public static void Case_1(int param_1)
        {
            Class_14.Case_1(param_1, param_2: null);
        }
    }
}