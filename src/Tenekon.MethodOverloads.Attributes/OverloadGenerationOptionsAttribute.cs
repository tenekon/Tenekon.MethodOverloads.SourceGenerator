#nullable enable
namespace Tenekon.MethodOverloads;

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
    MatchTarget,
    Public,
    Internal,
    Private
}

[global::System.AttributeUsage(
    global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct
    | global::System.AttributeTargets.Interface | global::System.AttributeTargets.Method)]
internal sealed class OverloadGenerationOptionsAttribute : global::System.Attribute
{
    public RangeAnchorMatchMode RangeAnchorMatchMode { get; set; }
    public OverloadSubsequenceStrategy SubsequenceStrategy { get; set; }
    public OverloadVisibility OverloadVisibility { get; set; }
    public global::System.Type? BucketType { get; set; }
}