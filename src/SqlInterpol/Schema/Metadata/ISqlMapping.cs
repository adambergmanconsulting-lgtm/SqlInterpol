namespace SqlInterpol.Schema;

/// <summary>
/// Defines the structural database mapping for a specific entity type, 
/// providing the physical table name, schema, and column overrides.
/// </summary>
public interface ISqlMapping
{
    /// <summary>
    /// Gets the physical name of the database table or view.
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// Gets the database schema the entity belongs to (e.g., "dbo"), 
    /// or an empty string if not explicitly configured.
    /// </summary>
    string SchemaName { get; }

    /// <summary>
    /// Gets the collection of explicit column mappings configured for this entity.
    /// </summary>
    IEnumerable<ISqlColumnMapping> Columns { get; }
}