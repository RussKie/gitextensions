`FormCheckoutBranch` builds the command with `Commands.CheckoutBranch(branchName, isRemote,
localChanges, newBranchMode, newBranchName)`, which returns an **`IGitCommand`**
(`ChangesRepoState = true`). It runs via `UICommands.StartCommandLineProcessDialog(...)` — the
**preferred structured path**. It handles local changes via `LocalChangesAction`
(merge/reset/stash) and can create/reset a new branch with tracking.

Difference from commit: checkout uses the **structured `IGitCommand` + `StartCommandLineProcessDialog`**
path, whereas commit uses the **legacy `ArgumentString` + `FormProcess.ShowDialog`** path. Both
refresh the UI via `RepoChangedNotifier` afterward.
