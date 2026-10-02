using SqlInterpol.Execution;
using SqlInterpol.Testing.Specifications;
using Xunit;

namespace SqlInterpol.Tests;

/// <summary>
/// Seam tests for <see cref="SqlBuilderExtensions.AppendLine(SqlBuilder, ISqlTemplate, object?)"/>.
/// </summary>
public class TemplateAppendLineEquivalenceTests
{
    [Fact]
    public void AppendLine_template_matches_Append_plus_newline()
    {
        var template = CompileSelectTemplate();

        var viaAppendLine = SqlBuilder.PostgreSql();
        viaAppendLine.AppendLine(template, new { CustId = 5 });
        var appendLineSql = viaAppendLine.Build().Sql;

        var viaAppend = SqlBuilder.PostgreSql();
        viaAppend.Append(template, new { CustId = 5 });
        viaAppend.AppendLine();
        var appendSql = viaAppend.Build().Sql;

        Assert.Equal(appendSql, appendLineSql);
        Assert.EndsWith(Environment.NewLine, appendLineSql);
    }

    [Fact]
    public void AppendLine_template_then_clause_keeps_trailing_newline_separator()
    {
        var template = CompileSelectTemplate();
        var db = SqlBuilder.PostgreSql();
        db.Entity<TemplateTestSuite.TemplateOrderModel>(out var o);

#pragma warning disable SQLIG10
        var sql = db.AppendLine(template, new { CustId = 5 })
            .Append($"ORDER BY {o.Id} DESC")
            .Build()
            .Sql;
#pragma warning restore SQLIG10

        Assert.Contains(Environment.NewLine + "ORDER BY", sql);
        Assert.Contains("$1", sql);
    }

    private static ISqlTemplate CompileSelectTemplate()
    {
        var db = SqlBuilder.PostgreSql();
        db.Entity<TemplateTestSuite.TemplateOrderModel>(out var o);

#pragma warning disable SQLIG10
        db.Template(out var template, $$"""
            SELECT {{o.Id}}, {{o.CustomerId}}
            FROM {{o}} AS o1
            WHERE {{o.CustomerId}} = {{Sql.Arg("CustId")}}
            """);
#pragma warning restore SQLIG10

        return template;
    }
}
