using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitCommands.ExternalLinks
{
    /// <summary>
    ///  Provides the ability to parse user-defined external link definitions.
    /// </summary>
    public interface IGitRevisionExternalLinksParser
    {
        /// <summary>
        ///  Retrieves all external links from the <paramref name="settings"/> applicable for the provided <paramref name="revision"/>.
        /// </summary>
        /// <param name="revision">The revision.</param>
        /// <param name="settings">The settings store to load the external link definitions from.</param>
        /// <returns>The collection of external links applicable to the revision.</returns>
        IEnumerable<ExternalLink> Parse(GitRevision revision, IDistributedSettingsSource settings);
    }

    internal sealed class GitRevisionExternalLinksParser : IGitRevisionExternalLinksParser
    {
        private readonly IConfiguredLinkDefinitionsProvider _effectiveLinkDefinitionsProvider;
        private readonly IExternalLinkRevisionParser _externalLinkRevisionParser;

        public GitRevisionExternalLinksParser(IConfiguredLinkDefinitionsProvider effectiveLinkDefinitionsProvider, IExternalLinkRevisionParser externalLinkRevisionParser)
        {
            _effectiveLinkDefinitionsProvider = effectiveLinkDefinitionsProvider;
            _externalLinkRevisionParser = externalLinkRevisionParser;
        }

        public IEnumerable<ExternalLink> Parse(GitRevision revision, IDistributedSettingsSource settings)
        {
            var definitions = _effectiveLinkDefinitionsProvider.Get(settings);
            return definitions.Where(definition => definition.Enabled)
                              .SelectMany(definition => _externalLinkRevisionParser.Parse(revision, definition));
        }
    }
}
