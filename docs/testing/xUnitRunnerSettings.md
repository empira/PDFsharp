# xUnit runner settings for test projects

## `xunit.runner.json`

Each test project can have an `xunit.runner.json` file, e.g.

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
