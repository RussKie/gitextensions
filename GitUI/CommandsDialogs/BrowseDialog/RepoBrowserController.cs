using GitCommands;
using GitCommands.Git;
using GitCommands.Gpg;
using GitCommands.UserRepositoryHistory;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.CommandsDialogs
{
    public interface IRepoBrowserController
    {
        void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption);

        Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision);
    }

    public class RepoBrowserController : IRepoBrowserController
    {
        private readonly IBrowseRepo _browseRepo;
        private readonly IGitGpgController _gitGpgController;
        private readonly IRepositoryCurrentBranchNameProvider _repositoryCurrentBranchNameProvider;
        private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;

        public RepoBrowserController(IServiceProvider serviceProvider)
        {
            _browseRepo = (IBrowseRepo)serviceProvider.GetService(typeof(IBrowseRepo));
            _gitGpgController = (IGitGpgController)serviceProvider.GetService(typeof(IGitGpgController));
            _repositoryCurrentBranchNameProvider = (IRepositoryCurrentBranchNameProvider)serviceProvider.GetService(typeof(IRepositoryCurrentBranchNameProvider));
            _invalidRepositoryRemover = (IInvalidRepositoryRemover)serviceProvider.GetService(typeof(IInvalidRepositoryRemover));
        }

        public void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption)
        {
            ToolStripMenuItem item = new(caption)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            menuItemContainer.DropDownItems.Add(item);

            item.Click += (obj, args) =>
            {
                OpenRepo(repo.Path);
            };

            if (repo.Path != caption)
            {
                item.ToolTipText = repo.Path;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                string branchName = _repositoryCurrentBranchNameProvider.GetCurrentBranchName(repo.Path);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                item.ShortcutKeyDisplayString = branchName;
            }).FileAndForget();
        }

        public async Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision)
        {
            if (!AppSettings.ShowGpgInformation.Value || revision?.ObjectId is null)
            {
                return null;
            }

            var getCommitSignature = _gitGpgController.GetRevisionCommitSignatureStatusAsync(revision);
            var getTagSignature = _gitGpgController.GetRevisionTagSignatureStatusAsync(revision);
            await Task.WhenAll(getCommitSignature, getTagSignature);

            var commitStatus = await getCommitSignature;
            var tagStatus = await getTagSignature;

            // Absence of Commit sign and Tag sign
            if (commitStatus == CommitStatus.NoSignature && tagStatus == TagStatus.NoTag)
            {
                return null;
            }

            return new GpgInfo(commitStatus,
                               _gitGpgController.GetCommitVerificationMessage(revision),
                               tagStatus,
                               _gitGpgController.GetTagVerifyMessage(revision));
        }

        private void OpenRepo(string repoPath)
        {
            if (Control.ModifierKeys != Keys.Control)
            {
                _browseRepo.SetWorkingDir(repoPath);
                return;
            }

            GitUICommands.LaunchBrowse(repoPath);
        }
    }
}
