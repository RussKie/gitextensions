using GitCommands.Settings;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitCommands.ExternalLinks
{
    /// <summary>
    ///  Provides the ability to retrieves available persisted external link definitions.
    /// </summary>
    public interface IConfiguredLinkDefinitionsProvider
    {
        /// <summary>
        ///  Loads all persisted external link definitions across all setting layers.
        /// </summary>
        IReadOnlyList<ExternalLinkDefinition> Get(IDistributedSettingsSource settings);
    }

    /// <summary>
    ///  Retrieves available persisted external link definitions.
    /// </summary>
    internal sealed class ConfiguredLinkDefinitionsProvider : IConfiguredLinkDefinitionsProvider
    {
        private readonly IExternalLinksStorage _externalLinksStorage;

        public ConfiguredLinkDefinitionsProvider(IExternalLinksStorage externalLinksStorage)
        {
            _externalLinksStorage = externalLinksStorage;
        }

        public IReadOnlyList<ExternalLinkDefinition> Get(IDistributedSettingsSource settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            DistributedSettings cachedSettings = new(null, settings.SettingsCache, SettingLevel.Unknown);
            IEnumerable<ExternalLinkDefinition>? effective = _externalLinksStorage.Load(cachedSettings);

            Validates.NotNull(effective);

            if (settings.LowerPriority is not null)
            {
                ConfiguredLinkDefinitionsProvider lowerPriorityLoader = new(_externalLinksStorage);
                effective = effective.Union(lowerPriorityLoader.Get(settings.LowerPriority));
            }

            return effective.ToList();
        }
    }
}
