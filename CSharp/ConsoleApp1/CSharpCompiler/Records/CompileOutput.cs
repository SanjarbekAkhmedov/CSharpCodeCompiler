using System.Reflection;

namespace General.Automation.Subscriptions.WebHooks.CSharpCompiler.Records
{
    public record CompileOutput
    {
        public Assembly Assembly { get; init; }
        public bool CompileSucceeded { get; init; }
        public string Errors { get; init; }
    }
}
