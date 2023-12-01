namespace CSharp.Services
{
    public interface IDotNetCodeAnalyzerService
    {
        Task<CodeAnalyzeOutput> AnalyzeCodeAsync(string code, int timeoutMilliseconds);
    }
}