namespace CSharp.Services
{
    public interface IDotNetFrameworkProvider
    {
        public DotNetVersionInfo GetDotNetVersion(string version, string language);
    }
}
