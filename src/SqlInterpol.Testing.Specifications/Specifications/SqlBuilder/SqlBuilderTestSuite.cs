using SqlInterpol.Configuration;
using SqlInterpol.Testing.Xunit;

namespace SqlInterpol.Testing.Specifications;

[SqlTestSuite(typeof(ISqlBuilderTestSuite))]
public abstract partial class SqlBuilderTestSuite
{
    [SqlGeneratorIgnore]
    public abstract SqlBuilder CreateBuilder(SqlInterpolOptions? options = null);

    [SqlTest(nameof(ISqlBuilderTestSuite.AppendData))]
    public void SqlBuilder_Append(SqlTestCase testCase)
    {
        var db = CreateBuilder();
        
        testCase.Act(() => 
        {
            db.Entity<Product>(out var p);
            return db.Append($"SELECT {p.Id}").Append($" FROM {p}").Build();
        });

        testCase.Assert();
    }

    [SqlTest(nameof(ISqlBuilderTestSuite.AppendLineData))]
    public void SqlBuilder_AppendLine(SqlTestCase testCase)
    {
        var db = CreateBuilder();
        
        testCase.Act(() => 
        {
            db.Entity<Product>(out var p);
            return db.AppendLine($"SELECT {p.Id}")
                     .Append($"FROM {p}")
                     .Build();
        });
        
        testCase.Assert();
    }

    [SqlTest(nameof(ISqlBuilderTestSuite.RawStringData))]
    public void SqlBuilder_RawString(SqlTestCase testCase)
    {
        var db = CreateBuilder();
        
        testCase.Act(() => 
        {
            db.Entity<Product>(out var p);
            return db.Append($$"""
                SELECT
                    {{p.Id}}
                FROM {{p}}
                """).Build();
        });

        testCase.Assert();
    }

    [SqlTest(nameof(ISqlBuilderTestSuite.FluentMappingData))]
    public void SqlBuilder_Renders_Fluent_Mapped_Names(SqlTestCase testCase)
    {
        // Arrange
        var options = new SqlInterpolOptions();
        options.Metadata
            .Entity<Order>(out var oConfig)
                .Table("tbl_orders")
                .Column(oConfig.Total, c => c.Name("order_total"));

        var db = CreateBuilder(options);

        // Act
        testCase.Act(() =>
        {
            db.Entity<Order>(out var o);

            return db.Append($$"""
                SELECT {{o.Id}}, {{o.Total}}
                FROM {{o}}
                """).Build();
        });

        // Assert
        testCase.Assert();
        db.AssertAotIntercepted();
    }
}