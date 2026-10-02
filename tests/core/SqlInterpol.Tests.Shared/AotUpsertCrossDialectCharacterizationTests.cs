using SqlInterpol.Configuration;
using SqlInterpol.Schema;
using SqlInterpol.Testing.Xunit;
using Xunit;

namespace SqlInterpol.Tests;

/// <summary>
/// Characterization: handwritten UPSERT is rewrite-correct under CrossDialect at runtime,
/// but the AOT interceptor intentionally falls back to JIT (SQLIG10) — see performance-aot.md.
/// </summary>
public class AotUpsertCrossDialectCharacterizationTests
{
    [SqlTable("Products", "dbo")]
    private sealed class Product
    {
        public int Id { get; set; }
        [SqlColumn("PROD_NAME")]
        public string Name { get; set; } = "";
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
    }

    [Fact]
    public void Handwritten_OnConflict_SqlServer_transpiles_to_MERGE_via_JIT()
    {
        var options = new SqlInterpolOptions { CrossDialectSqlTranspilation = true };
        var db = SqlBuilder.SqlServer(options);
        var newProduct = new { Id = 42, Name = "Apple", CategoryId = 1, Price = 10m };
        var updateProduct = new { Name = "Apple", Price = 10m };

        db.Entity<Product>(out var p);

#pragma warning disable SQLIG10
        var result = db.Append($$"""
            INSERT INTO {{p}} {{newProduct}}
            ON CONFLICT {{p.Id}}
            DO UPDATE SET {{updateProduct}}
            """).Build();
#pragma warning restore SQLIG10

        Assert.Contains("MERGE INTO", result.Sql, StringComparison.OrdinalIgnoreCase);
        db.AssertJitFallback();
    }

    [Fact]
    public void Handwritten_OnConflict_PostgreSql_stays_OnConflict_via_JIT()
    {
        var options = new SqlInterpolOptions { CrossDialectSqlTranspilation = true };
        var db = SqlBuilder.PostgreSql(options);
        var newProduct = new { Id = 42, Name = "Apple", CategoryId = 1, Price = 10m };
        var updateProduct = new { Name = "Apple", Price = 10m };

        db.Entity<Product>(out var p);

#pragma warning disable SQLIG10
        var result = db.Append($$"""
            INSERT INTO {{p}} {{newProduct}}
            ON CONFLICT {{p.Id}}
            DO UPDATE SET {{updateProduct}}
            """).Build();
#pragma warning restore SQLIG10

        Assert.Contains("ON CONFLICT", result.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MERGE INTO", result.Sql, StringComparison.OrdinalIgnoreCase);
        db.AssertJitFallback();
    }
}
