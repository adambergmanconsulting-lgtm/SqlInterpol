namespace SqlInterpol.Schema;

/// <summary>
/// Represents a explicit mapping between a CLR property on an entity model and a physical database column.
/// </summary>
public interface ISqlColumnMapping
{
    /// <summary>
    /// Gets the name of the CLR property on the entity model.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets the physical name of the mapped database column.
    /// </summary>
    string ColumnName { get; }
}