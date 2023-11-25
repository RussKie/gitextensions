namespace GitCommands.Git;

public static partial class Commands
{
    public record RebaseOptions
    {
        public string? BranchName { get; init; }
        public bool Interactive { get; init; }
        public bool PreserveMerges { get; init; }
        public bool AutoSquash { get; init; }
        public bool AutoStash { get; init; }
        public bool IgnoreDate { get; init; }
        public bool CommitterDateIsAuthorDate { get; init; }
        public bool? UpdateRefs { get; init; }
        public string? From { get; init; }
        public string? OnTo { get; init; }
        public bool SupportRebaseMerges { get; init; } = true;
    }
}
