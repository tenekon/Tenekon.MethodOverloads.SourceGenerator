#nullable enable
namespace Tenekon.MethodOverloads;

[global::Microsoft.CodeAnalysis.Embedded]
internal enum RangeAnchorMatchMode
{
    TypeOnly,
    TypeAndName
}

[global::Microsoft.CodeAnalysis.Embedded]
internal enum OverloadSubsequenceStrategy
{
    PrefixOnly,
    UniqueBySignature
}

[global::Microsoft.CodeAnalysis.Embedded]
internal enum OverloadVisibility
{
    MatchTarget,
    Public,
    Internal,
    Private
}

[global::Microsoft.CodeAnalysis.Embedded]
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