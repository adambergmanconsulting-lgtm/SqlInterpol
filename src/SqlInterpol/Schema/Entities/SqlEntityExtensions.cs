using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using SqlInterpol.Segments;

namespace SqlInterpol.Schema;

/// <summary>
/// Provides fluent extension methods for SQL entities.
/// </summary>
public static class SqlEntityExtensions
{
    /// <summary>
    /// Dynamically defers a SQL column reference using a string property name.
    /// This allows dynamic field projection without requiring string indexers on POCO models.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="propertyName">The name of the property to resolve.</param>
    /// <returns>A dynamic column fragment.</returns>
    public static SqlDynamicColumnFragment Column<T>(this T entity, string propertyName) where T : class
    {
        return new SqlDynamicColumnFragment(typeof(T), propertyName);
    }

    /// <summary>
    /// Safely resolves a dynamic column reference using a strongly-typed property access, avoiding expression tree allocations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TProp">The type of the property being accessed.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="property">The property access (e.g., <c>p.CategoryId</c>).</param>
    /// <param name="propertyExpression">Automatically captured by the compiler (e.g., <c>"p.CategoryId"</c>).</param>
    /// <returns>A dynamic column fragment.</returns>
    public static SqlDynamicColumnFragment Column<T, TProp>(
        this T entity, 
        TProp property, 
        [CallerArgumentExpression("property")] string? propertyExpression = null) where T : class
    {
        return entity.Column(ExtractPropertyName(propertyExpression));
    }

    /// <summary>
    /// Creates a dynamic SQL ORDER BY fragment using a string property name (Default Direction).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="propertyName">The name of the property to sort by.</param>
    /// <returns>An order fragment.</returns>
    public static ISqlOrderFragment OrderBy<T>(this T entity, string propertyName) where T : class
    {
        return new SqlDynamicOrderFragment(new SqlDynamicColumnFragment(typeof(T), propertyName));
    }

    /// <summary>
    /// Creates a dynamic SQL ORDER BY fragment using a string property name with an explicit direction.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="propertyName">The name of the property to sort by.</param>
    /// <param name="direction">The sort direction.</param>
    /// <returns>An order fragment.</returns>
    public static ISqlOrderFragment OrderBy<T>(this T entity, string propertyName, SqlOrderDirection direction) where T : class
    {
        return new SqlDynamicOrderFragment(new SqlDynamicColumnFragment(typeof(T), propertyName), direction);
    }

    /// <summary>
    /// Creates a SQL ORDER BY fragment using a strongly-typed property access (Default Direction), avoiding expression tree allocations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TProp">The type of the property being accessed.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="property">The property access (e.g., <c>p.CategoryId</c>).</param>
    /// <param name="propertyExpression">Automatically captured by the compiler.</param>
    /// <returns>An order fragment.</returns>
    public static ISqlOrderFragment OrderBy<T, TProp>(
        this T entity, 
        TProp property, 
        [CallerArgumentExpression("property")] string? propertyExpression = null) where T : class
    {
        return entity.OrderBy(ExtractPropertyName(propertyExpression));
    }

    /// <summary>
    /// Creates a SQL ORDER BY fragment using a strongly-typed property access with an explicit direction, avoiding expression tree allocations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TProp">The type of the property being accessed.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="property">The property access (e.g., <c>p.CategoryId</c>).</param>
    /// <param name="direction">The sort direction.</param>
    /// <param name="propertyExpression">Automatically captured by the compiler.</param>
    /// <returns>An order fragment.</returns>
    public static ISqlOrderFragment OrderBy<T, TProp>(
        this T entity, 
        TProp property, 
        SqlOrderDirection direction, 
        [CallerArgumentExpression("property")] string? propertyExpression = null) where T : class
    {
        return entity.OrderBy(ExtractPropertyName(propertyExpression), direction);
    }

    /// <summary>
    /// Creates a SQL ORDER BY fragment using a strongly-typed member expression (Default Direction).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="expression">The property selector expression.</param>
    /// <returns>An order fragment.</returns>
    public static ISqlOrderFragment OrderBy<T>(this T entity, Expression<Func<T, object?>> expression) where T : class
    {
        return entity.OrderBy(SqlExpressionHelper.GetPropertyName(expression));
    }

    /// <summary>
    /// Creates a SQL ORDER BY fragment using a strongly-typed member expression with an explicit direction.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="expression">The property selector expression.</param>
    /// <param name="direction">The sort direction.</param>
    /// <returns>An order fragment.</returns>
    public static ISqlOrderFragment OrderBy<T>(this T entity, Expression<Func<T, object?>> expression, SqlOrderDirection direction) where T : class
    {
        return entity.OrderBy(SqlExpressionHelper.GetPropertyName(expression), direction);
    }

    private static string ExtractPropertyName(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return string.Empty;

        var lastDot = expression!.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < expression.Length - 1)
        {
            return expression.Substring(lastDot + 1).Trim();
        }

        return expression.Trim();
    }
}