using System.Runtime.CompilerServices;
using SqlInterpol.Configuration;

namespace SqlInterpol.Schema;

/// <summary>
/// The root builder for fluently configuring database schema mappings without polluting POCOs with attributes.
/// </summary>
public class SqlMetadataBuilder
{
    private readonly SqlInterpolOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMetadataBuilder"/> class.
    /// </summary>
    /// <param name="options">The options instance where configured mappings will be registered.</param>
    public SqlMetadataBuilder(SqlInterpolOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Configures the database schema mapping for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The POCO entity type to configure.</typeparam>
    /// <param name="dummy">An uninitialized instance of the entity, used to enable strongly-typed property tracking without allocations.</param>
    /// <param name="varName">Automatically populated by the compiler with the variable name of the dummy instance.</param>
    /// <returns>A specialized builder to fluently map tables, views, and columns for this entity.</returns>
    public SqlEntityMetadataBuilder<T> Entity<T>(out T dummy, [CallerArgumentExpression("dummy")] string? varName = null)
    {
        if (typeof(T).IsValueType) dummy = default!;
        else dummy = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

        var config = (SqlEntityConfiguration?)_options.Mappings
            .Find(m => m is SqlEntityConfiguration e && e.EntityType == typeof(T));
            
        if (config == null)
        {
            config = new SqlEntityConfiguration(typeof(T));
            _options.Mappings.Add(config);
        }

        return new SqlEntityMetadataBuilder<T>(this, config);
    }
}