`FormCommit` hosts the commit UI. Clicking *Commit* calls `CheckForStagedAndCommit` → inner
`DoCommit()`, which:

1. Validates state — aborts on `InTheMiddleOfConflictedMerge()`, empty/invalid message, or
   prompts on detached HEAD.
2. Persists the message via `CommitMessageManager`.
3. Runs `ScriptEvent.BeforeCommit` scripts (can cancel).
4. Builds the command with `Commands.Commit(...)`, which returns an **`ArgumentString`** (the
   legacy pattern — NOT `IGitCommand`).
5. Runs it via `FormProcess.ShowDialog(...)`.
6. Calls `RepoChangedNotifier.Notify()` to refresh, then runs `ScriptEvent.AfterCommit` scripts.
