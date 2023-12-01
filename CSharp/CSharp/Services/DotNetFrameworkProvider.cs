namespace CSharp.Services
{
    using CSharp;
    using Extensions;
    internal class DotNetFrameworkProvider : IDotNetFrameworkProvider
    {
        private List<DotNetVersionInfo>? _allVersions;
        private List<string>? _languages;

        private List<DotNetVersionInfo> AllVersions
        {
            get
            {
                _allVersions ??= ReadVersionsFromFolder();
                return _allVersions;
            }
        }

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

        private void SetMetaDataAndUsing(DotNetVersionInfo dotNetVersionInfo, string versionPath)
        {
            var defaultUsingPath = Path.Combine(versionPath, Extensions.DEFAULT_USING_PATH_NAME);
            dotNetVersionInfo.DefaultUsing = File.ReadAllLines(defaultUsingPath);

            var metaDataPath = Path.Combine(versionPath, Extensions.META_DATA_PATH_NAME);
            dotNetVersionInfo.AvailableMetaData = Directory.GetFiles(metaDataPath).ToList();
        }

        public DotNetVersionInfo GetDotNetVersion(string version, string language) => AllVersions.First(x => x.Version == version && x.Language == language);
    }
}
