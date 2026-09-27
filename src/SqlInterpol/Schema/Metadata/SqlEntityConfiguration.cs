namespace SqlInterpol.Schema;

/// <summary>
/// Holds the fluent configuration overrides for a specific entity type, 
/// taking precedence over attribute-based and convention-based mappings.
/// </summary>
public class SqlEntityConfiguration : ISqlMapping
{
    /// <summary>
    /// Gets the underlying CLR type that this configuration applies to.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// Gets or sets the explicit physical name of the table or view.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the explicit database schema the entity belongs to.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the structural type override (e.g., forcing a class to be treated as a View).
    /// </summary>
    public SqlEntityType? EntityTypeOverride { get; set; }

    /// <summary>
    /// Gets the dictionary mapping CLR property names to their physical database column names.
    /// </summary>
    public Dictionary<string, string> ColumnMappings { get; } = new(StringComparer.OrdinalIgnoreCase);

    // ISqlMapping Implementation
    string ISqlMapping.TableName => TableName ?? EntityType.Name;
    string ISqlMapping.SchemaName => SchemaName ?? string.Empty;
    IEnumerable<ISqlColumnMapping> ISqlMapping.Columns => 
        ColumnMappings.Select(kv => new SqlColumnMapping(kv.Key, kv.Value));

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlEntityConfiguration"/> class.
    /// </summary>
    /// <param name="entityType">The CLR model type being configured.</param>
    public SqlEntityConfiguration(Type entityType)
    {
        EntityType = entityType;
    }
}