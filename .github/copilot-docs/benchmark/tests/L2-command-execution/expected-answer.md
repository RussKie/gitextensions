git runs through the process layer: `IExecutable`/`Executable` (`Start` → `IProcess`), with the
`ExecutableExtensions` helpers `GetOutput` / `RunCommand` / `ExecuteAsync` returning an
`ExecutionResult`. `GitModule` is the per-repository facade most code goes through.

Two command patterns:

- **Structured (preferred):** `Commands.<Verb>(...)` returns an `IGitCommand` carrying
  `AccessesRemote` and `ChangesRepoState`. Run via
  `GitUICommands.StartCommandLineProcessDialog(owner, cmd)`, which picks the right process dialog
  and fires `RepoChangedNotifier`. Used for state-changing UI ops (e.g. `CheckoutBranch`,
  `DeleteBranch`, `CreateTag`).
- **Legacy (argument-only):** some `Commands` methods (e.g. `Commit`) return an `ArgumentString`,
  run via `FormProcess.ShowDialog(...)`.

Arguments are always built with `ArgumentBuilder`/`GitArgumentBuilder`, never string concatenation.
