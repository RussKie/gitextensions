using System.Collections.Generic;
using System.Linq;
using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitUI.Script
{
    public sealed class DistributedScriptsManager
    {
        private readonly DistributedSettings _cachedSettings;
        private readonly DistributedScriptsManager? _lowerPriority;
        private readonly IUserScriptsStorage _externalLinksStorage = new UserScriptsStorage();
        private readonly List<ScriptInfo>? _scripts;

        public DistributedScriptsManager(DistributedSettings settings)
        {
            _cachedSettings = new DistributedSettings(null, settings.SettingsCache, settings.SettingLevel);
            _scripts = _externalLinksStorage.Load(_cachedSettings).ToList();

            if (settings.LowerPriority is not null)
            {
                _lowerPriority = new DistributedScriptsManager(settings.LowerPriority);
            }
        }

        /// <summary>
        ///  Gets the minimum script ID for each settings level.
        /// </summary>
        public int MinimumUserScriptID => _cachedSettings.SettingLevel switch
        {
            SettingLevel.Global => 9_000,
            SettingLevel.Distributed => 10_000,
            _ => 11_000
        };

        /// <summary>
        ///  Adds the provided script definition at the lowest available level.
        /// </summary>
        /// <param name="script">Script definition.</param>
        public void Add(ScriptInfo script)
        {
            if (_lowerPriority is null || _lowerPriority.Contains(script.HotkeyCommandIdentifier))
            {
                if (!Contains(script.HotkeyCommandIdentifier))
                {
                    _scripts.Add(script);
                }

                // TODO: else notify the user?
            }
            else
            {
                _lowerPriority.Add(script);
            }
        }

        /// <summary>
        ///  Checks if a script definition with the supplied ID exists in any level of available settings.
        /// </summary>
        /// <param name="scriptId">The script with ID to find.</param>
        /// <returns><see langword="true"/> if a script definition already exists; otherwise <see langword="false"/>.</returns>
        private bool Contains(int scriptId)
        {
            return _scripts.Any(scriptInfo => scriptInfo.HotkeyCommandIdentifier == scriptId);
        }

        /// <summary>
        ///  Loads all script definitions from all available levels.
        /// </summary>
        /// <returns>A collection of all available definitions.</returns>
        public IReadOnlyList<ScriptInfo> GetEffectiveSettings()
        {
            List<ScriptInfo> scripts = _scripts
                .Union(_lowerPriority?.GetEffectiveSettings() ?? Enumerable.Empty<ScriptInfo>())
                .ToList();

            // Ensure script IDs are unique; update if necessary.
            // NB: this loop is in O(n^2), but we don't expect more than few dozens of scripts, so it should be acceptable.
            HashSet<int> ids = new();
            foreach (var script in scripts)
            {
                if (!ids.Add(script.HotkeyCommandIdentifier))
                {
                    script.HotkeyCommandIdentifier = scripts.Select(s => s.HotkeyCommandIdentifier).Max() + 1;
                }
            }

            return scripts;
        }

        /// <summary>
        ///  Removes the supplied script definition.
        /// </summary>
        /// <param name="script">Script definition.</param>
        public void Remove(ScriptInfo script)
        {
            if (_lowerPriority is null || _lowerPriority.Contains(script.HotkeyCommandIdentifier))
            {
                for (int index = 0; index < _scripts.Count; index++)
                {
                    if (_scripts[index].HotkeyCommandIdentifier == script.HotkeyCommandIdentifier)
                    {
                        _scripts.RemoveAt(index);
                    }
                }

                // TODO: else notify the user?
            }
            else
            {
                _lowerPriority?.Remove(script);
            }
        }

        /// <summary>
        ///  Saves the script definitions to the settings.
        /// </summary>
        public void Save()
        {
            _lowerPriority?.Save();
            _externalLinksStorage.Save(_cachedSettings, _scripts);
        }

        /// <summary>
        ///  Removes the supplied script definition.
        /// </summary>
        /// <param name="script">Script definition.</param>
        public void Update(ScriptInfo script)
        {
            if (_lowerPriority is null || _lowerPriority.Contains(script.HotkeyCommandIdentifier))
            {
                for (int index = 0; index < _scripts.Count; index++)
                {
                    if (_scripts[index].HotkeyCommandIdentifier == script.HotkeyCommandIdentifier)
                    {
                        _scripts[index] = script;
                    }
                }

                // TODO: else notify the user?
            }
            else
            {
                _lowerPriority.Update(script);
            }
        }
    }
}
