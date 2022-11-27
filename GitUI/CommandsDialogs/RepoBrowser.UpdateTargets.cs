﻿namespace GitUI.CommandsDialogs
{
    public sealed partial class RepoBrowser
    {
        [Flags]
        private enum UpdateTargets
        {
            None = 1,
            DiffList = 2,
            FileTree = 4,
            CommitInfo = 8
        }
    }
}
