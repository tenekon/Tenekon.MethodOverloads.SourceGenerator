#nullable enable
namespace Tenekon.MethodOverloads;

[global::Microsoft.CodeAnalysis.Embedded]
[global::System.AttributeUsage(
    global::System.AttributeTargets.Method
    | global::System.AttributeTargets.Class
    | global::System.AttributeTargets.Interface,
    AllowMultiple = true)]
internal sealed class SupplyParameterTypeAttribute : global::System.Attribute
{
    public SupplyParameterTypeAttribute(string typeParameterName, global::System.Type suppliedType)
    {
        TypeParameterName = typeParameterName;
        SuppliedType = suppliedType;
    }

    public string TypeParameterName { get; }

    public global::System.Type SuppliedType { get; }

    public object? Group { get; set; }
}
