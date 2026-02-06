namespace Tenekon.MethodOverloads.SourceGenerator;

internal static class GeneratorAttributesSource
{
    public const string GenerateOverloadsAttribute = """
        #nullable enable
        namespace Tenekon.MethodOverloads.SourceGenerator;

        [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
        [global::Microsoft.CodeAnalysis.Embedded]
        public sealed class GenerateOverloadsAttribute : global::System.Attribute
        {
            public GenerateOverloadsAttribute()
            {
            }

            public GenerateOverloadsAttribute(string beginEnd)
            {
                Begin = beginEnd;
                End = beginEnd;
            }

            /// <summary>
            /// All parameters beginning from <see cref=\"Begin\"/> (inclusive) are considered for optional or required.
            /// </summary>
            public string? Begin { get; set; }

            /// <summary>
            /// All parameters after <see cref=\"BeginExclusive\"/> are considered for optional or required.
            /// </summary>
            public string? BeginExclusive { get; set; }

            /// <summary>
            /// All parameters before <see cref=\"EndExclusive\"/> are considered for optional or required.
            /// </summary>
            public string? EndExclusive { get; set; }

            /// <summary>
            /// All parameters until <see cref=\"End\"/> (inclusive) are considered for optional or required.
            /// </summary>
            public string? End { get; set; }

            public global::System.Type[]? Matchers { get; set; }
        }
        """;

    public const string GenerateMethodOverloadsAttribute = """
        #nullable enable
        namespace Tenekon.MethodOverloads.SourceGenerator;

        [global::System.AttributeUsage(
            global::System.AttributeTargets.Class
            | global::System.AttributeTargets.Struct
            | global::System.AttributeTargets.Interface,
            AllowMultiple = true)]
        [global::Microsoft.CodeAnalysis.Embedded]
        public sealed class GenerateMethodOverloadsAttribute : global::System.Attribute
        {
            public global::System.Type[]? Matchers { get; set; }
        }
        """;

    public const string OverloadGenerationOptionsAttribute = """
        #nullable enable
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
            MatchTarget,
            Public,
            Internal,
            Private
        }

        [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct | global::System.AttributeTargets.Interface | global::System.AttributeTargets.Method)]
        [global::Microsoft.CodeAnalysis.Embedded]
        public sealed class OverloadGenerationOptionsAttribute : global::System.Attribute
        {
            public RangeAnchorMatchMode RangeAnchorMatchMode { get; set; }
            public OverloadSubsequenceStrategy SubsequenceStrategy { get; set; }
            public OverloadVisibility OverloadVisibility { get; set; }
        }
        """;

    public const string MatcherUsageAttribute = """
        #nullable enable
        namespace Tenekon.MethodOverloads.SourceGenerator;

        [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true)]
        [global::Microsoft.CodeAnalysis.Embedded]
        internal sealed class MatcherUsageAttribute : global::System.Attribute
        {
            public MatcherUsageAttribute(string methodName)
            {
            }
        }
        """;
}