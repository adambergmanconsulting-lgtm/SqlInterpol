namespace SqlInterpol.Schema;

/// <summary>
/// Provides fluent methods to configure schema mappings for a specific column.
/// </summary>
public class SqlColumnMetadataBuilder
{
    private readonly SqlEntityConfiguration _config;
    private readonly string _propertyName;

    internal SqlColumnMetadataBuilder(SqlEntityConfiguration config, string propertyName)
    {
        _config = config;
        _propertyName = propertyName;
    }

    /// <summary>
    /// Maps the property to a specific physical database column name.
    /// </summary>
    /// <param name="name">The physical database column name.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public SqlColumnMetadataBuilder Name(string name)
    {
        _config.ColumnMappings[_propertyName] = name;
        return this;
    }
}