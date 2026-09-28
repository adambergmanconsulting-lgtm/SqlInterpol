using System.Data;
using Npgsql;
using SqlInterpol.Configuration;
using SqlInterpol.Dialects;
using SqlInterpol.E2E.Tests.Framework;
using SqlInterpol.Testing.Specifications;
using Xunit;

namespace SqlInterpol.E2E.Tests.Dialects.PostgreSql;

public class PostgreSqlFixture : E2EDatabaseTestSuiteBase
{
    protected override SqlDialectKind Dialect => SqlDialectKind.PostgreSql;

    protected override IDbConnection CreateConnection() => 
        new NpgsqlConnection(E2EConfig.GetConnectionString(SqlDialectKind.PostgreSql));

    protected override SqlBuilder ConfigureBuilder(SqlInterpolOptions? options = null) => 
        SqlBuilder.PostgreSql(options);
}

// 1. Inherit directly from FromTestSuite and implement IClassFixture explicitly
[Trait("Category", "PostgreSQL")]
public class PostgreSqlFromSpecs : FromTestSuite, IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public PostgreSqlFromSpecs(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public override SqlBuilder CreateBuilder(SqlInterpolOptions? options = null) 
        => _fixture.CreateBuilder(options);
}