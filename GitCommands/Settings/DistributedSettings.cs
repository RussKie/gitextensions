using System;
using System.Diagnostics;
using System.IO;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    /// <summary>
    /// Settings that can be distributed with repository.
    /// They can be overridden for a particular repository.
    /// </summary>
    [DebuggerDisplay("Level = {" + nameof(SettingLevel) + ",nq}, Cache = {" + nameof(SettingsCache) + ",nq}")]
    public class DistributedSettings : SettingsContainer<DistributedSettings, GitExtSettingsCache>
    {
        public DistributedSettings(DistributedSettings? lowerPriority, GitExtSettingsCache settingsCache, SettingLevel settingLevel)
            : base(lowerPriority, settingsCache)
        {
            SettingLevel = settingLevel;
        }

        #region CreateXXX

        public static DistributedSettings CreateEffective(GitModule module)
        {
            return CreateLocal(module, CreateDistributed(module, CreateGlobal()), SettingLevel.Effective);
        }

        private static DistributedSettings CreateLocal(GitModule module, DistributedSettings? lowerPriority,
            SettingLevel settingLevel, bool allowCache = true)
        {
            ////if (module.IsBareRepository()
            return new DistributedSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.GitCommonDirectory, AppSettings.SettingsFileName), allowCache),
                settingLevel);
        }

        public static DistributedSettings CreateLocal(GitModule module, bool allowCache = true)
        {
            return CreateLocal(module, null, SettingLevel.Local, allowCache);
        }

        private static DistributedSettings CreateDistributed(GitModule module, DistributedSettings? lowerPriority, bool allowCache = true)
        {
            return new DistributedSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.WorkingDir, AppSettings.SettingsFileName), allowCache),
                SettingLevel.Distributed);
        }

        public static DistributedSettings CreateDistributed(GitModule module, bool allowCache = true)
        {
            return CreateDistributed(module, null, allowCache);
        }

        public static DistributedSettings CreateGlobal(bool allowCache = true)
        {
            return new DistributedSettings(null, GitExtSettingsCache.Create(AppSettings.SettingsFilePath, allowCache),
                SettingLevel.Global);
        }

        #endregion

        public override void SetValue<T>(string name, T value, Func<T, string?> encode)
        {
            bool isEffectiveLevel = LowerPriority?.LowerPriority is not null;
            bool isDetachedOrGlobal = LowerPriority is null;

            if (isDetachedOrGlobal || SettingsCache.HasValue(name))
            {
                // there is no lower level
                // or the setting is assigned on this level
                SettingsCache.SetValue(name, value, encode);
            }
            else if (isEffectiveLevel)
            {
                // Settings stored at the Distributed level always have to be set directly
                // so I do not pass the control to the LowerPriority(Distributed)
                // in order to not overwrite the setting
                if (LowerPriority!.SettingsCache.HasValue(name))
                {
                    // if the setting is set at the Distributed level, do not overwrite it
                    // instead of that, set the setting at the Local level to make it effective
                    // but only if the effective value is different from the new value
                    if (LowerPriority!.SettingsCache.HasADifferentValue(name, value, encode))
                    {
                        SettingsCache.SetValue(name, value, encode);
                    }
                }
                else
                {
                    // if the setting isn't set at the Distributed level, do not set it there
                    // instead of that, set the setting at the Global level (it becomes effective then)
                    LowerPriority!.LowerPriority!.SetValue(name, value, encode);
                }
            }
            else
            {
                // the settings is not assigned on this level, recurse to the lower level
                LowerPriority!.SetValue(name, value, encode);
            }
        }
    }
}
