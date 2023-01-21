using GitUIPluginInterfaces;

namespace GitCommands.Gpg
{
    public interface IGpgInfoProvider
    {
        Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision);
    }

    public class GpgInfoProvider : IGpgInfoProvider
    {
        private readonly IGitGpgController _gitGpgController;

        public GpgInfoProvider(IGitGpgController gitGpgController)
        {
            _gitGpgController = gitGpgController;
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
    }
}
