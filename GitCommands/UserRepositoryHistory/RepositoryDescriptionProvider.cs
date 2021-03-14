using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GitCommands.Git;
using GitExtUtils;

namespace GitCommands.UserRepositoryHistory
{
    public interface IRepositoryDescriptionProvider
    {
        /// <summary>
        /// Returns a short name for repository.
        /// If the repository contains a description it is returned,
        /// otherwise the last part of path is returned.
        /// </summary>
        /// <param name="repositoryDir">Path to repository.</param>
        /// <returns>Short name for repository.</returns>
        string Get(string repositoryDir);
    }

    [Export(typeof(IRepositoryDescriptionProvider))]
    public sealed class RepositoryDescriptionProvider : IRepositoryDescriptionProvider
    {
        private const string RepositoryDescriptionFileName = "description";
        private const string DefaultDescription = "Unnamed repository; edit this file 'description' to name the repository.";

#pragma warning disable CS0649
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Import(typeof(IGitDirectoryResolver))]
        private readonly IGitDirectoryResolver _gitDirectoryResolver;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS0649

        /// <summary>
        /// Returns a short name for repository.
        /// If the repository contains a description it is returned,
        /// otherwise the last part of path is returned.
        /// </summary>
        /// <param name="repositoryDir">Path to repository.</param>
        /// <returns>Short name for repository.</returns>
        public string Get(string repositoryDir)
        {
            var dirInfo = new DirectoryInfo(repositoryDir);
            if (!dirInfo.Exists)
            {
                return dirInfo.Name;
            }

            string? desc = ReadRepositoryDescription(repositoryDir);
            if (!Strings.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }

            return dirInfo.Name;
        }

        /// <summary>
        /// Reads repository description's first line from ".git\description" file.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <returns>If the repository has description, returns that description, else returns <c>null</c>.</returns>
        private string? ReadRepositoryDescription(string workingDir)
        {
            var gitDir = _gitDirectoryResolver.Resolve(workingDir);
            var descriptionFilePath = Path.Combine(gitDir, RepositoryDescriptionFileName);

            if (!File.Exists(descriptionFilePath))
            {
                return null;
            }

            try
            {
                var repositoryDescription = File.ReadLines(descriptionFilePath).FirstOrDefault();
                return string.Equals(repositoryDescription, DefaultDescription, StringComparison.CurrentCulture)
                    ? null
                    : repositoryDescription;
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
