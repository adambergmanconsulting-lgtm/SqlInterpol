using SqlInterpol.Configuration;
using SqlInterpol.Dialects;
using SqlInterpol.Pipeline;
using SqlInterpol.Segments;
using SqlInterpol.Testing.Xunit.Dialects;
using Xunit;

namespace SqlInterpol.Tests;

/// <summary>
/// Focused seam tests for <see cref="SqlFeatureGate"/> after extract from <see cref="SqlBuilder"/>.
/// </summary>
public class SqlFeatureGateTests
{
    private sealed class EmptyFeaturesDialect : ISqlDialect
    {
        private readonly MockDialect _inner = new();

        public SqlDialectKind Kind => _inner.Kind;
        public string OpenQuote => _inner.OpenQuote;
        public string CloseQuote => _inner.CloseQuote;
        public string ParameterPrefix => _inner.ParameterPrefix;
        public int QueryParametersMaxCount => _inner.QueryParametersMaxCount;
        public IReadOnlySet<SqlFeature> SupportedFeatures { get; } = new HashSet<SqlFeature>();
        public bool IsExpressionContext(string keyword) => _inner.IsExpressionContext(keyword);
        public string QuoteIdentifier(string identifier) => _inner.QuoteIdentifier(identifier);
        public string UnquoteIdentifier(string identifier) => _inner.UnquoteIdentifier(identifier);
        public string QuoteEntityName(string name, string? schema) => _inner.QuoteEntityName(name, schema);
        public string GetParameterName(int index) => _inner.GetParameterName(index);
        public string ApplyAlias(string expression, string? alias) => _inner.ApplyAlias(expression, alias);
        public string RenderFragment(ISqlFragment fragment, ISqlContext context) =>
            _inner.RenderFragment(fragment, context);
    }

    [Fact]
    public void EnsureSupported_allows_untagged_segments()
    {
        var segments = new[]
        {
            new SqlSegment(SqlSegmentType.Literal, "SELECT 1")
        };

        SqlFeatureGate.EnsureSupported(new EmptyFeaturesDialect(), segments);
    }

    [Fact]
    public void EnsureSupported_throws_for_unsupported_tag()
    {
        var segments = new[]
        {
            new SqlSegment(SqlSegmentType.Literal, "FOR UPDATE", null, SqlSegmentTag.ForUpdateKeyword)
        };

        var ex = Assert.Throws<SqlDialectException>(
            () => SqlFeatureGate.EnsureSupported(new EmptyFeaturesDialect(), segments));

        Assert.Contains("FOR UPDATE", ex.Message);
    }

    [Fact]
    public void EnsureSupported_throws_for_ISqlFeatureRequirement()
    {
        var segments = new[]
        {
            new SqlSegment(SqlSegmentType.Raw, new SqlLockFragment(SqlLockMode.Share))
        };

        var ex = Assert.Throws<SqlDialectException>(
            () => SqlFeatureGate.EnsureSupported(new EmptyFeaturesDialect(), segments));

        Assert.Contains("FOR SHARE", ex.Message);
    }

    [Fact]
    public void Build_SqLite_ForUpdate_still_fails_via_public_Build()
    {
        var db = SqlBuilder.SqLite();
        db.Entity<ProductStub>(out var p);

        var ex = Assert.Throws<SqlDialectException>(() =>
            db.Append($$"""
                SELECT {{p.Id}}
                FROM {{p}} FOR UPDATE
                """).Build());

        Assert.Contains("FOR UPDATE", ex.Message);
    }

    private sealed class ProductStub
    {
        public int Id { get; set; }
    }
}
