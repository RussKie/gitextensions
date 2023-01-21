namespace GitUI.CommandsDialogs
{
    [Flags]
    internal enum UpdateTargets
    {
        None = 1,
        DiffList = 2,
        FileTree = 4,
        CommitInfo = 8
    }
}
