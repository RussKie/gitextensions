using GitCommands.Settings;

namespace GitUI.ScriptsEngine
{
    /// <summary>
    ///  Provides the ability to manage user scripts.
    /// </summary>
    public interface IScriptsManager
    {
        /// <summary>
        ///  Adds the provided script definition at the lowest available settings level.
        /// </summary>
        /// <param name="script">Script definition.</param>
        void Add(ScriptInfo script);

        /// <summary>
        ///  Retrieves the script definition by <paramref name="scriptId"/>.
        /// </summary>
        /// <param name="scriptId">The script ID.</param>
        /// <returns>The script definition, if found; <see langword="null"/>, otherwise.</returns>
        ScriptInfo? GetScript(int scriptId);

        /// <summary>
        ///  Loads all script definitions from all available settings levels.
        /// </summary>
        /// <returns>A collection of all available definitions.</returns>
        IReadOnlyList<ScriptInfo> GetScripts();

        /// <summary>
        ///  Initializes the script manager instance from the supplied <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The settings store.</param>
        /// <remarks>This method must be called before all others.</remarks>
        void Initialize(DistributedSettings settings);

        /// <summary>
        ///  Removes the supplied script definition from the list.
        /// </summary>
        /// <param name="script">Script definition.</param>
        void Remove(ScriptInfo script);

        /// <summary>
        ///  Saves the currently loaded script definitions to the settings.
        /// </summary>
        void Save();

        /// <summary>
        ///  Updates the existing script definition at the lowest available settings level matching.
        /// </summary>
        /// <param name="script">The script definition to update.</param>
        void Update(ScriptInfo script);
    }
}
