using System.Collections.Concurrent;
using System.Reflection;
using SqlInterpol.Configuration;

namespace SqlInterpol.Schema;

/// <summary>
/// A thread-safe static registry that caches reflection metadata, property maps, and 
/// compiled argument getters for mapped entities and dynamic templates.
/// </summary>
public static class SqlMetadataRegistry
{
    private static readonly ConcurrentDictionary<Type, SqlEntityMetadata> _reflectionCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _dtoPropertyCache = new();
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, Func<object, object?>>> _getterCache = new();

    /// <summary>
    /// Bridges the active context options to internal pipeline extensions that call parameterless metadata lookups.
    /// Safely isolates the configuration per execution flow without thread pollution.
    /// </summary>
    public static readonly AsyncLocal<SqlInterpolOptions?> ActiveOptions = new();

    public static SqlEntityMetadata GetMetadata<T>() => GetMetadata(typeof(T), ActiveOptions.Value);
    
    public static SqlEntityMetadata GetMetadata(Type type) => GetMetadata(type, ActiveOptions.Value);

    public static SqlEntityMetadata GetMetadata<T>(SqlInterpolOptions? options) => GetMetadata(typeof(T), options);

    public static SqlEntityMetadata GetMetadata(Type type, SqlInterpolOptions? options)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        // 1. Get the highly optimized, shared reflection baseline
        var baseline = GetCachedReflectionMetadata(type);

        if (options == null || options.Mappings == null || options.Mappings.Count == 0)
        {
            return baseline;
        }

        // 2. Discover mapping cleanly using strong typing
        var fluentConfig = options.Mappings
            .OfType<SqlEntityConfiguration>()
            .FirstOrDefault(m => m.EntityType == type);

        if (fluentConfig == null)
        {
            return baseline;
        }

        // 3. Merge table/schema overrides
        string finalName = fluentConfig.TableName ?? baseline.Name;
        string? finalSchema = fluentConfig.SchemaName ?? baseline.Schema;
        SqlEntityType finalType = fluentConfig.EntityTypeOverride ?? baseline.Type;

        var finalColumns = new Dictionary<PropertyInfo, string>(baseline.Columns);
        
        // 4. Map columns, actively stripping CallerArgumentExpression prefixes (e.g. "c.Id" -> "Id")
        foreach (var mapping in fluentConfig.ColumnMappings)
        {
            string propName = mapping.Key;
            
            int dotIdx = propName.LastIndexOf('.');
            if (dotIdx >= 0) 
            {
                propName = propName.Substring(dotIdx + 1);
            }

            var prop = baseline.Columns.Keys.FirstOrDefault(p => 
                p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
            
            if (prop != null)
            {
                finalColumns[prop] = mapping.Value;
            }
        }

        return new SqlEntityMetadata(finalName, finalSchema, finalType, finalColumns);
    }

    private static SqlEntityMetadata GetCachedReflectionMetadata(Type type)
    {
        return _reflectionCache.GetOrAdd(type, t =>
        {
            string name = t.Name;
            string? schema = null;
            SqlEntityType entityType = SqlEntityType.Table;

            var tableAttr = t.GetCustomAttribute<SqlTableAttribute>();
            if (tableAttr != null)
            {
                name = tableAttr.Name ?? t.Name;
                schema = tableAttr.Schema;
                entityType = SqlEntityType.Table;
            }
            else
            {
                var viewAttr = t.GetCustomAttribute<SqlViewAttribute>();
                if (viewAttr != null)
                {
                    name = viewAttr.Name ?? t.Name;
                    schema = viewAttr.Schema;
                    entityType = SqlEntityType.View;
                }
            }

            var columns = new Dictionary<PropertyInfo, string>();
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute<SqlIgnoreAttribute>() != null) continue;

                var propType = prop.PropertyType;
                if (propType.IsClass && propType != typeof(string) && propType != typeof(byte[])) continue;

                var colAttr = prop.GetCustomAttribute<SqlColumnAttribute>();
                columns[prop] = colAttr?.Name ?? prop.Name;
            }

            return new SqlEntityMetadata(name, schema, entityType, columns);
        });
    }

    public static PropertyInfo[] GetDtoProperties(Type type)
    {
        return _dtoPropertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => 
            {
                if (p.GetCustomAttribute<SqlIgnoreAttribute>() != null) return false;
                var propType = p.PropertyType;
                if (propType.IsClass && propType != typeof(string) && propType != typeof(byte[])) return false;
                return true;
            })
            .ToArray());
    }

    public static IReadOnlyDictionary<string, Func<object, object?>> GetArgumentGetters(Type type)
    {
        return _getterCache.GetOrAdd(type, t =>
        {
            var dict = new Dictionary<string, Func<object, object?>>(StringComparer.OrdinalIgnoreCase);
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var localProp = prop;
                dict[prop.Name] = obj => localProp.GetValue(obj);
            }
            return dict;
        });
    }
}