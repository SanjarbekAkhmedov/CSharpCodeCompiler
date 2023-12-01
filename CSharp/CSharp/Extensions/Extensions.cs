using System.Reflection;

namespace CSharp.Extensions
{
    public static class Extensions
    {
        public const string DEFAULT_USING_PATH_NAME = "DefaultUsings.txt";
        public const string META_DATA_PATH_NAME = "MetaData";
        public const string CSHARP_LANGUAGE = "CSharp";
        public const string DOTNET_SIX_VERSION = "NET7";
        public const string RepositoryResourceFolder = "Resources";

        public static string GetLanguagesPath() => Path.Combine(GetResourcesPath(), "Languages");

        public static string GetLanguagePath(string language) => Path.Combine(GetLanguagesPath(), language);

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

        public static string GetDirectoryName(this string path) => new DirectoryInfo(path).Name;
    }
}
