namespace General.Automation.Subscriptions.WebHooks.CSharpCompiler.Repositories
{
    using Extensions;
    using General.Automation.Subscriptions.WebHooks.CSharpCompiler.Records;

    public interface IDotNetFrameworkProvider
    {
        /// <summary>
        /// Gets information about a specific .NET framework version for a given language.
        /// </summary>
        public DotNetVersionInfo GetDotNetVersion(string version, string language);
    }

    public class DotNetFrameworkProvider : IDotNetFrameworkProvider
    {
        private List<DotNetVersionInfo> _allVersions;
        private List<string> _languages;

        /// <summary>
        /// Gets the list of all available .NET framework versions for different languages.
        /// </summary>
        private List<DotNetVersionInfo> AllVersions
        {
            get
            {
                _allVersions ??= ReadVersionsFromFolder();
                return _allVersions;
            }
        }

        /// <summary>
        /// Gets the list of supported languages based on available language folders.
        /// </summary>
        private List<string> Languages
        {
            get
            {
                _languages ??= Directory.GetDirectories(Extensions.GetLanguagesPath())
                                       .Select(s => s.GetDirectoryName())
                                       .ToList();
                return _languages;
            }
        }

        /// <summary>
        /// Reads and returns a list of .NET framework versions for each language from the designated folders.
        /// </summary>
        private List<DotNetVersionInfo> ReadVersionsFromFolder()
        {
            var versions = new List<DotNetVersionInfo>();

            foreach (var language in Languages)
            {
                var versionsPath = Path.Combine(Extensions.GetLanguagePath(language), "Versions");
                var allVersions = Directory.GetDirectories(versionsPath);

                foreach (var version in allVersions)
                {
                    var name = version.GetDirectoryName();
                    var dotNetVersionInfo = new DotNetVersionInfo { Language = language, Version = name };
                    SetMetaDataAndUsing(dotNetVersionInfo, version);
                    versions.Add(dotNetVersionInfo);
                }
            }

            return versions;
        }

        /// <summary>
        /// Sets metadata and default using for a specific .NET framework version.
        /// </summary>
        private void SetMetaDataAndUsing(DotNetVersionInfo dotNetVersionInfo, string versionPath)
        {
            var defaultUsingPath = Path.Combine(versionPath, Extensions.DEFAULT_USING_PATH_NAME);
            dotNetVersionInfo.DefaultUsing = File.ReadAllLines(defaultUsingPath);

            var metaDataPath = Path.Combine(versionPath, Extensions.META_DATA_PATH_NAME);
            dotNetVersionInfo.AvailableMetaData = Directory.GetFiles(metaDataPath).ToList();
        }

        /// <summary>
        /// Gets information about a specific .NET framework version for a given language.
        /// </summary>
        public DotNetVersionInfo GetDotNetVersion(string version, string language) => AllVersions.First(x => x.Version == version && x.Language == language);
    }
}
