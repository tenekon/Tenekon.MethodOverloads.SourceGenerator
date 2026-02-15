using System;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct GroupKey(string? TypeDisplay, string? ConstantKey)
{
    public static GroupKey Default => new(null, null);

    public bool IsDefault => TypeDisplay is null && ConstantKey is null;

    public static GroupKey FromTypeDisplay(string display)
    {
        return new GroupKey(display, ConstantKey: null);
    }

    public static GroupKey FromConstant(string key)
    {
        return new GroupKey(TypeDisplay: null, key);
    }

    public string ToKeyString()
    {
        if (IsDefault) return "default";
        if (TypeDisplay is not null) return "type:" + TypeDisplay;
        return "const:" + ConstantKey;
    }
}
