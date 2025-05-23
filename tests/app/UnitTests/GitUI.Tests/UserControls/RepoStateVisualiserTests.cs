﻿using GitExtensions.Extensibility.Git;
using GitUI.UserControls;

namespace GitUITests.UserControls
{
    [TestFixture]
    public sealed class RepoStateVisualiserTests
    {
        [SetUp]
        public void SetUp()
        {
            _repoStateVisualiser = new RepoStateVisualiser();
        }

        private RepoStateVisualiser _repoStateVisualiser;

        private static GitItemStatus CreateGitItemStatus(
            bool isStaged = false,
            bool isTracked = true,
            bool isSubmodule = false)
        {
            return new GitItemStatus("file1")
            {
                Staged = isStaged ? StagedStatus.Index : StagedStatus.WorkTree,
                IsTracked = isTracked,
                IsSubmodule = isSubmodule
            };
        }

        [Test]
        public void ReturnsIconCleanWhenThereIsNoChangedFiles()
        {
            (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(Array.Empty<GitItemStatus>());

            ClassicAssert.AreEqual(RepoStateVisualiser.Clean, commitIcon);
        }

        [Test]
        public void ReturnsIconDirtySubmodulesWhenThereAreOnlyWorkTreeSubmodules()
        {
            (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus(isSubmodule: true)
            });

            ClassicAssert.AreEqual(RepoStateVisualiser.DirtySubmodules, commitIcon);
        }

        [Test]
        public void ReturnsIconDirtyWhenThereAreWorkTreeChanges()
        {
            (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus()
            });

            ClassicAssert.AreEqual(RepoStateVisualiser.Dirty, commitIcon);
        }

        [Test]
        public void ReturnsIconMixedWhenThereAreIndexAndWorkTreeFiles()
        {
            (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus()
            });

            ClassicAssert.AreEqual(RepoStateVisualiser.Mixed, commitIcon);
        }

        [Test]
        public void ReturnsIconStagedWhenThereAreOnlyIndexFiles()
        {
            (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus(isStaged: true)
            });

            ClassicAssert.AreEqual(RepoStateVisualiser.Staged, commitIcon);
        }

        [Test]
        public void ReturnsIconUntrackedOnlyWhenThereAreUntrackedFilesOnly()
        {
            (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isTracked: false),
                CreateGitItemStatus(isTracked: false)
            });

            ClassicAssert.AreEqual(RepoStateVisualiser.UntrackedOnly, commitIcon);
        }

        [Test]
        public void ReturnsIconUnknownWhenNull()
        {
            (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(null);

            ClassicAssert.AreEqual(RepoStateVisualiser.Unknown, commitIcon);
        }
    }
}
