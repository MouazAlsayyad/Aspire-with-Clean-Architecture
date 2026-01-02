namespace AspireApp.Domain.Shared.Common;

/// <summary>
/// Attribute to exclude entities, properties, or fields from change tracking and logging.
/// When applied to a class, the entire entity will be excluded from logging.
/// When applied to a property or field, only that property/field will be excluded from logging.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ExcludeFromLoggingAttribute : Attribute
{
}

