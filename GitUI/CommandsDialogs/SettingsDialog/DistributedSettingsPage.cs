using GitCommands.Settings;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class DistributedSettingsPage : SettingsPageWithHeader, IRepoDistSettingsPage
    {
        protected DistributedSettingsSet? DistributedSettingsSet => CommonLogic.DistributedSettingsSet;
        protected DistributedSettings? CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CurrentSettings = DistributedSettingsSet?.EffectiveSettings;
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            Validates.NotNull(CurrentSettings);

            return CurrentSettings;
        }

        public void SetEffectiveSettings()
        {
            if (DistributedSettingsSet is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.EffectiveSettings);
            }
        }

        public void SetLocalSettings()
        {
            if (DistributedSettingsSet is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.LocalSettings);
            }
        }

        public override void SetGlobalSettings()
        {
            if (DistributedSettingsSet is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.GlobalSettings);
            }
        }

        public void SetRepoDistSettings()
        {
            if (DistributedSettingsSet is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.RepoDistSettings);
            }
        }

        private void SetCurrentSettings(DistributedSettings settings)
        {
            if (CurrentSettings is not null)
            {
                SaveSettings();
            }

            CurrentSettings = settings;

            LoadSettings();
        }
    }
}
