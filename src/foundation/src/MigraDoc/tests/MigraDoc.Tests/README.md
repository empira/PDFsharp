# MigraDoc tests

Tests related to MigraDoc.

Requirements:
* ...

## `xunit.runner.json`

Each test project can have a `xunit.runner.json`, e.g.

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",

  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "preEnumerateTheories": true
}
```

Notes
* parallelizeAssembly can be set to false when needed (e.g. when testing with a database)
* parallelizeTestCollections can be set to false when needed (e.g. when testing with a database)
