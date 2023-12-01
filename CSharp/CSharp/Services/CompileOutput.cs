using System.Reflection;

namespace CSharp.Services
{
    public record CompileOutput
    {
        public Assembly? Assembly { get; init; }

        public bool CompileSucceeded { get; init; }

        public string? Errors { get; init; }
    }
}
