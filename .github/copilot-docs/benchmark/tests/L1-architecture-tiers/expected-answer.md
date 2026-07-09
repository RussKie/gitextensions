Git Extensions is layered into four tiers with a strict one-way dependency direction:

1. **GitExtensions.Extensibility** — contracts/primitives: `IGitModule`, `IExecutable`,
   `IGitCommand`, `ArgumentBuilder`. (Versioned public plugin surface.)
2. **GitCommands** — the git engine: `GitModule`, `Executable`, `Commands`, settings/config.
3. **GitUI** — WinForms UI: `FormBrowse`, dialogs, `RevisionGrid`, `GitUICommands`.
4. **GitExtensions** (exe) — bootstrap + DI entry point.

Dependency arrow: `Extensibility ← GitCommands ← GitUI ← GitExtensions(exe)`. It never reverses
(e.g. `GitCommands` must not reference `GitUI`). Off to the sides: **plugins** under `src/plugins`
(via `GitUIPluginInterfaces`) and **native** code under `src/native` (the Explorer shell extension).
