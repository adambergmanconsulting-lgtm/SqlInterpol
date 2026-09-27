using System.Data;
using Npgsql;
using SqlInterpol.Configuration;
using SqlInterpol.Dialects;
using SqlInterpol.E2E.Tests.Framework;
using Xunit;

namespace SqlInterpol.E2E.Tests.Dialects.PostgreSql;

[Trait("Category", "PostgreSQL")]
public class PostgreSqlE2ETests
{
    public class FromSpecs(Fixture fixture) : E2EFromTestSuite<Fixture>(fixture);
    
    // Add future suites easily:
    // public class WhereSpecs(Fixture fixture) : E2EWhereTestSuite<Fixture>(fixture);

    public class Fixture : E2EDatabaseTestSuiteBase
    {
        // Tells the base class where to find the .sql scripts
        protected override SqlDialectKind Dialect => SqlDialectKind.PostgreSql;

        protected override IDbConnection CreateConnection() => 
            new NpgsqlConnection(E2EConfig.GetConnectionString(SqlDialectKind.PostgreSql));

        protected override SqlBuilder ConfigureBuilder(SqlInterpolOptions? options = null) => 
            SqlBuilder.PostgreSql(options);
    }
}