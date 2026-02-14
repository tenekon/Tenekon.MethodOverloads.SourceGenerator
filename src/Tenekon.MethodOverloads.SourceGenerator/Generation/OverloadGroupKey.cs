namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal readonly record struct OverloadGroupKey(
    string Namespace,
    string BucketName,
    bool IsDefault);
