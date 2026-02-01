// ReSharper disable once CheckNamespace
namespace Tenekon.MethodOverloads.SourceGenerator;

public enum RangeAnchorMatchMode
{
    TypeOnly,
    TypeAndName
}

public enum OverloadSubsequenceStrategy
{
    PrefixOnly,
    UniqueBySignature
}

public enum OverloadVisibility
{
    MatchTarget = 1,
    Public = 2,
    Internal = 3,
    // InternalProtected, // Reserved
    // Protected // Reserved
    // Private // Reserved
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OverloadGenerationOptionsAttribute : Attribute
{
    public RangeAnchorMatchMode RangeAnchorMatchMode { get; set; }
    public OverloadSubsequenceStrategy SubsequenceStrategy { get; set; }
    public OverloadVisibility OverloadVisibility { get; set; }
}


