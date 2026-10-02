# Product backlog notes

Operational Rank 5 hygiene: keep this list honest against shipped code. Prefer CANONICAL task rows + tests over undated bullets.

## Open

* Fix SqlTemplate after new simplified syntax — needs a repro / failing characterization before code change ([templates-caching.md](docs/templates-caching.md))

## Done / retired

* Add AppendUpsert — shipped (`SqlBuilderExtensions.AppendUpsert` + `IUpsertTestSuite` / `UpsertTemplateData`)
* Unit tests for templates and Append… methods — template suites exist; `TemplateAppendLineEquivalenceTests` covers `AppendLine(ISqlTemplate)`
* EntityAutoAliasing = true by default — rejected (violates WYSIWYG)
* Characterize AOT×CrossDialect handwritten UPSERT — shipped as tests + honest docs
* AOT-intercept handwritten UPSERT/ON CONFLICT — shipped 2026-10-02 (structural GetSegment emit; CrossDialect rewrite remains at `Build()`; `AssertAotIntercepted` on `AotUpsertCrossDialectCharacterizationTests`) ([performance-aot.md](docs/performance-aot.md))
