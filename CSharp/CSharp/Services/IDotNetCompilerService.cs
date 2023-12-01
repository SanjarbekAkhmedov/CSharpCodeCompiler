namespace CSharp.Services
{
    public interface IDotNetCompilerService
    {
        public DotNetVersionInfo DotNetVersionInfo { get; }
        public CompileOutput CompileText(string code);
    }
}
