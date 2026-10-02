using SqlInterpol.Configuration;
using SqlInterpol.Schema;
using SqlInterpol.Testing.Xunit;
using Xunit;

namespace SqlInterpol.Tests;

/// <summary>
/// Characterization: handwritten UPSERT is AOT-intercepted while CrossDialect rewrite
/// still runs in the shared Build() pipeline (not compile-time MERGE in the emitter).
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
    public void Handwritten_OnConflict_SqlServer_transpiles_to_MERGE_via_Aot()
    {
        var options = new SqlInterpolOptions { CrossDialectSqlTranspilation = true };
        var db = SqlBuilder.SqlServer(options);
        var newProduct = new { Id = 42, Name = "Apple", CategoryId = 1, Price = 10m };
        var updateProduct = new { Name = "Apple", Price = 10m };

        db.Entity<Product>(out var p);

        var result = db.Append($$"""
            INSERT INTO {{p}} {{newProduct}}
            ON CONFLICT {{p.Id}}
            DO UPDATE SET {{updateProduct}}
            """).Build();

        Assert.Contains("MERGE INTO", result.Sql, StringComparison.OrdinalIgnoreCase);
        db.AssertAotIntercepted();
    }

    [Fact]
    public void Handwritten_OnConflict_PostgreSql_stays_OnConflict_via_Aot()
    {
        var options = new SqlInterpolOptions { CrossDialectSqlTranspilation = true };
        var db = SqlBuilder.PostgreSql(options);
        var newProduct = new { Id = 42, Name = "Apple", CategoryId = 1, Price = 10m };
        var updateProduct = new { Name = "Apple", Price = 10m };

        db.Entity<Product>(out var p);

        var result = db.Append($$"""
            INSERT INTO {{p}} {{newProduct}}
            ON CONFLICT {{p.Id}}
            DO UPDATE SET {{updateProduct}}
            """).Build();

        Assert.Contains("ON CONFLICT", result.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MERGE INTO", result.Sql, StringComparison.OrdinalIgnoreCase);
        db.AssertAotIntercepted();
    }
}
