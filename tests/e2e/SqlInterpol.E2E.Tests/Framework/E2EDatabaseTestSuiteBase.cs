using System.Data;
using SqlInterpol.Configuration;
using SqlInterpol.Dialects;
using SqlInterpol.Testing.Xunit;
using Xunit;

namespace SqlInterpol.E2E.Tests.Framework;

public abstract class E2EDatabaseTestSuiteBase : IAsyncLifetime
{
    protected abstract IDbConnection CreateConnection();
    protected abstract SqlBuilder ConfigureBuilder(SqlInterpolOptions? options);
    
    protected abstract SqlDialectKind Dialect { get; }

    // Made public so the Test Classes can check if it's null to trigger a Skip
    public IDbConnection? Connection { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            Connection = CreateConnection();
            Connection.Open();

            foreach (var script in GetSchemaScripts())
            {
                if (string.IsNullOrWhiteSpace(script)) continue;

                using var command = Connection.CreateCommand();
                command.CommandText = script;
                command.ExecuteNonQuery();
            }
        }
        catch (Exception)
        {
            // Database is offline or failed to initialize. 
            // Gracefully swallow the exception so xUnit can hit Assert.Skip() in the test classes.
            Connection = null;
        }

        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Connection?.Dispose();
        return Task.CompletedTask;
    }

    public SqlBuilder CreateBuilder(SqlInterpolOptions? options = null)
    {
        SqlTestCase.CurrentConnection.Value = Connection;
        return ConfigureBuilder(options);
    }

    protected virtual IEnumerable<string> GetSchemaScripts()
    {
        var scriptDir = Path.Combine("Dialects", Dialect.Value, "Scripts");

        var initPath = Path.Combine(scriptDir, "00_Init.sql");
        if (File.Exists(initPath))
        {
            yield return File.ReadAllText(initPath);
        }

        var modelDir = Path.Combine(scriptDir, "Models");
        if (Directory.Exists(modelDir))
        {
            // CRITICAL: Directory.GetFiles does not guarantee order on all OS platforms.
            // OrderBy ensures your 10_ schemas run before 11_ data files!
            var modelFiles = Directory.GetFiles(modelDir, "*.sql").OrderBy(f => f);
            foreach (var file in modelFiles)
            {
                yield return File.ReadAllText(file);
            }
        }
    }
}