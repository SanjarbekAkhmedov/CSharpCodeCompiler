namespace General.Automation.Subscriptions.WebHooks.CSharpCompiler.Extensions
{
    public static class Extensions
    {
        public const string DEFAULT_USING_PATH_NAME = "DefaultUsings.txt";
        public const string META_DATA_PATH_NAME = "MetaData";
        public const string CSHARP_LANGUAGE = "CSharp";
        public const string DOTNET_SIX_VERSION = "NET7";
        public const string RepositoryResourceFolder = "Resources";

        /// <summary>
        /// Gets the path to the "Languages" folder within the resources directory.
        /// </summary>
        public static string GetLanguagesPath() =>
            Path.Combine(GetResourcesPath(), "Languages");

        /// <summary>
        /// Gets the path to the language-specific folder within the "Languages" directory.
        /// </summary>
        public static string GetLanguagePath(string language) =>
            Path.Combine(GetLanguagesPath(), language);

        /// <summary>
        /// Gets the path to the resources directory based on the location of the current class.
        /// </summary>
        public static string GetResourcesPath()
        {
            var directory = Path.GetDirectoryName(typeof(Extensions).Assembly.Location);
            var repositoryResourcePath = SearchParentDirectories(directory, RepositoryResourceFolder);
            if (repositoryResourcePath != null)
                return Path.Combine(repositoryResourcePath, RepositoryResourceFolder);
            return string.Empty;

            string SearchParentDirectories(string startPath, string targetFolder)
            {
                var currentPath = startPath;
                while (!string.IsNullOrEmpty(currentPath))
                {
                    var folderPath = Path.Combine(currentPath, targetFolder);
                    if (Directory.Exists(folderPath))
                        return currentPath;
                    currentPath = Path.GetDirectoryName(currentPath);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the name of the directory from a given path.
        /// </summary>
        public static string GetDirectoryName(this string path) =>
            new DirectoryInfo(path).Name;
    }
}
