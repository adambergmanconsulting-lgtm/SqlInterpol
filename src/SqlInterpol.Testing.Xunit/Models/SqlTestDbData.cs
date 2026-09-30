using System.Text.Json;

namespace SqlInterpol.Testing.Xunit;

/// <summary>
/// Represents the expected database state and structure for end-to-end (E2E) test validation.
/// Used to assert column schema, affected row counts, and specific data results.
/// </summary>
public sealed class SqlTestDbData
{
    /// <summary>
    /// Gets or initializes the exact column names expected to be returned by the database, in order.
    /// </summary>
    public string[]? Columns { get; init; }

    /// <summary>
    /// Gets or initializes the expected number of rows returned by a query or affected by a mutation.
    /// </summary>
    public int? RowCount { get; init; }

    /// <summary>
    /// Gets or initializes an optional SQL query executed immediately after a mutation (INSERT, UPDATE, DELETE) 
    /// to fetch and verify the resulting state of the database.
    /// </summary>
    public string? VerifySql { get; init; }

    /// <summary>
    /// Gets or initializes the JSON-serialized representation of the expected data.
    /// Used primarily to support xUnit's cross-AppDomain serialization requirements.
    /// </summary>
    public string? DataJson { get; init; }

    /// <summary>
    /// Sets the expected records and automatically serializes them to JSON for xUnit cross-domain safety.
    /// </summary>
    public IEnumerable<object>? Data 
    { 
        init => DataJson = value != null ? JsonSerializer.Serialize(value) : null; 
    }
}