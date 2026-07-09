Make the label a `TranslationString` (or set the control text on a `Translate`-derived
form/control so the tooling discovers it). Then:

1. Build first (`dotnet build /v:q`) — the generator reflects over the built assemblies.
2. Run `update-loc.cmd` from the repo root; it runs `TranslationApp`, regenerates
   `English.xlf`, and stages it.
3. Commit the regenerated `English.xlf` **in the same commit** as the code change.

Hard rules: never hand-edit `.xlf` files (they are generated); CI fails if `English.xlf` is stale.
