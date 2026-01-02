namespace AspireApp.Domain.Shared.Common;

/// <summary>
/// Marker interface for aggregate roots in DDD.
/// Aggregate roots are entities that serve as the entry point to an aggregate.
/// They ensure consistency boundaries and are the only entities that can be loaded from repositories.
/// </summary>
public interface IAggregateRoot
{
}

