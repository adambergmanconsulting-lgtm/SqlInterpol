namespace SqlInterpol.Schema;

/// <summary>
/// Represents a concrete mapping between a CLR property and a physical database column.
/// </summary>
/// <param name="propertyName">The name of the CLR property.</param>
/// <param name="columnName">The physical name of the mapped database column.</param>
public class SqlColumnMapping(string propertyName, string columnName) : ISqlColumnMapping
{
    /// <inheritdoc />
    public string PropertyName { get; } = propertyName;

    /// <inheritdoc />
    public string ColumnName { get; } = columnName;
}