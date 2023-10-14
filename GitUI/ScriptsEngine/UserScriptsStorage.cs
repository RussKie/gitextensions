using System.Xml;
using System.Xml.Serialization;
using GitCommands.Settings;

namespace GitUI.ScriptsEngine
{
    /// <summary>
    ///  Provides the ability to read and save user scripts.
    /// </summary>
    public interface IUserScriptsStorage
    {
        /// <summary>
        ///  Loads user scripts from the <paramref name="settings"/>.
        /// </summary>
        IReadOnlyList<ScriptInfo> Load(DistributedSettings settings);

        /// <summary>
        ///  Saves the <paramref name="scripts"/> to the <paramref name="settings"/>.
        /// </summary>
        void Save(DistributedSettings settings, IReadOnlyList<ScriptInfo> scripts);
    }

    internal sealed class UserScriptsStorage : IUserScriptsStorage
    {
        private const string SettingName = "ownScripts";
        private static readonly XmlSerializer _serializer = new(typeof(List<ScriptInfo>));

        public IReadOnlyList<ScriptInfo>? Load(DistributedSettings settings)
        {
            string xml = settings.GetString(SettingName, null);
            IReadOnlyList<ScriptInfo> scripts = LoadFromXmlString(xml);
            return scripts;
        }

        public void Save(DistributedSettings settings, IReadOnlyList<ScriptInfo> scripts)
        {
            string? xml;
            if (scripts.Count == 0)
            {
                xml = null;
            }
            else
            {
                StringWriter sw = new();
                _serializer.Serialize(sw, scripts);
                xml = sw.ToString();
            }

            settings.SetString(SettingName, xml);
        }

        // TODO: refactor and outsource to the centralised SettingsSerializer implementations.
        private static IReadOnlyList<ScriptInfo> LoadFromXmlString(string? xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return Array.Empty<ScriptInfo>();
            }

            using StringReader stringReader = new(xmlString);
            using XmlTextReader xmlReader = new(stringReader);
            return (List<ScriptInfo>)_serializer.Deserialize(xmlReader);
        }
    }
}
