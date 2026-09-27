using System;
using System.Runtime.CompilerServices;

namespace SqlInterpol.Schema;

/// <summary>
/// Provides fluent methods to configure schema, table names, and column overrides for a specific entity type.
/// </summary>
/// <typeparam name="T">The CLR model type being configured.</typeparam>
public class SqlEntityMetadataBuilder<T>
{
    private readonly SqlMetadataBuilder _parent;
    private readonly SqlEntityConfiguration _config;

    internal SqlEntityMetadataBuilder(SqlMetadataBuilder parent, SqlEntityConfiguration config)
    {
        _parent = parent;
        _config = config;
    }

    /// <summary>
    /// Maps the entity to a specific physical database table name.
    /// </summary>
    /// <param name="name">The name of the database table.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public SqlEntityMetadataBuilder<T> Table(string name)
    {
        _config.TableName = name;
        _config.EntityTypeOverride = SqlInterpol.Schema.SqlEntityType.Table;
        return this;
    }

    /// <summary>
    /// Maps the entity to a specific database schema (e.g., "dbo").
    /// </summary>
    /// <param name="name">The name of the database schema.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public SqlEntityMetadataBuilder<T> Schema(string name)
    {
        _config.SchemaName = name;
        return this;
    }

    /// <summary>
    /// Maps the entity to a specific physical database view name.
    /// </summary>
    /// <param name="name">The name of the database view.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public SqlEntityMetadataBuilder<T> View(string name)
    {
        _config.TableName = name;
        _config.EntityTypeOverride = SqlInterpol.Schema.SqlEntityType.View;
        return this;
    }

    /// <summary>
    /// Configures the mapping for a specific property on the entity using a nested builder.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
    /// <param name="property">The property access expression (e.g., <c>p.Name</c>).</param>
    /// <param name="configure">An action to configure the column mapping details.</param>
    /// <param name="propertyExpression">The compiler-injected string representing the property access.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public SqlEntityMetadataBuilder<T> Column<TProp>(
        TProp property, 
        Action<SqlColumnMetadataBuilder> configure, 
        [CallerArgumentExpression("property")] string? propertyExpression = null)
    {
        string propName = ExtractPropertyName(propertyExpression);
        
        var colBuilder = new SqlColumnMetadataBuilder(_config, propName);
        configure(colBuilder);

        return this;
    }

    /// <summary>
    /// A convenient shorthand for mapping a property directly to a column name.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being mapped.</typeparam>
    /// <param name="property">The property access expression (e.g., <c>p.Name</c>).</param>
    /// <param name="columnName">The physical database column name.</param>
    /// <param name="propertyExpression">The compiler-injected string representing the property access.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public SqlEntityMetadataBuilder<T> Column<TProp>(
        TProp property, 
        string columnName, 
        [CallerArgumentExpression("property")] string? propertyExpression = null)
    {
        return Column(property, c => c.Name(columnName), propertyExpression);
    }

    /// <summary>
    /// Pivots the configuration chain to begin mapping a brand new entity type.
    /// Allows multiple entities to be mapped in a single fluent statement.
    /// </summary>
    /// <typeparam name="TOther">The next CLR model type to configure.</typeparam>
    /// <param name="dummy">An uninitialized reference used to strongly type the fluent configuration chain.</param>
    /// <param name="varName">The compiler-injected variable name used for scoping.</param>
    /// <returns>A new builder scoped to <typeparamref name="TOther"/>.</returns>
    public SqlEntityMetadataBuilder<TOther> Entity<TOther>(out TOther dummy, [CallerArgumentExpression("dummy")] string? varName = null)
    {
        return _parent.Entity(out dummy, varName);
    }

    private static string ExtractPropertyName(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Could not resolve property name from expression.");

        // The compiler passes "p.Name". We only want "Name".
        var lastDot = expression.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < expression.Length - 1)
        {
            return expression.Substring(lastDot + 1).Trim();
        }

        return expression.Trim();
    }
}