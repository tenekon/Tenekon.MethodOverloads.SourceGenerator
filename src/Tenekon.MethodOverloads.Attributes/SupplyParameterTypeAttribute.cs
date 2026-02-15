#nullable enable
namespace Tenekon.MethodOverloads;

[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
public sealed class SupplyParameterTypeAttribute : global::System.Attribute
{
    public SupplyParameterTypeAttribute(string typeParameterName, global::System.Type suppliedType)
    {
        TypeParameterName = typeParameterName;
        SuppliedType = suppliedType;
    }

    public string TypeParameterName { get; }

    public global::System.Type SuppliedType { get; }
}
