# Product backlog notes

Operational Rank 5 hygiene: keep this list honest against shipped code. Prefer CANONICAL task rows + tests over undated bullets.

## Open

* Fix SqlTemplate after new simplified syntax — needs a repro / failing characterization before code change ([templates-caching.md](docs/templates-caching.md))
* Honor CrossDialectSqlTranspilation **in the AOT emitter** for UPSERT — characterized 2026-10-02 (`AotUpsertCrossDialectCharacterizationTests`: CrossDialect MERGE/ON CONFLICT works via **JIT**; `SQLIG10`). Next slice: emit without fallback + `AssertAotIntercepted` on upsert suites ([performance-aot.md](docs/performance-aot.md))

## Done / retired

* Add AppendUpsert — shipped (`SqlBuilderExtensions.AppendUpsert` + `IUpsertTestSuite` / `UpsertTemplateData`)
* Unit tests for templates and Append… methods — template suites exist; `TemplateAppendLineEquivalenceTests` covers `AppendLine(ISqlTemplate)`
* EntityAutoAliasing = true by default — rejected (violates WYSIWYG)
* Characterize AOT×CrossDialect handwritten UPSERT — shipped as tests + honest docs (JIT fallback documented); full AOT emit still open above
