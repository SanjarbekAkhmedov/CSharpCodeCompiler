using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;

namespace General.Automation.Subscriptions.WebHooks.CSharpCompiler.Services
{
    using Extensions;
    using General.Automation.Subscriptions.WebHooks.CSharpCompiler.Repositories;
    using General.Automation.Subscriptions.WebHooks.CSharpCompiler.Records;
    using Microsoft.CodeAnalysis;
    using System.Text;
    using System.Reflection;

    public interface IDotNetCompilerService 
    {
        /// <summary>
        /// Compiles the provided C# code into a dynamic assembly.
        /// </summary>
        public CompileOutput CompileText(string code);
    }

    public class CSharpCodeCompilerService : IDotNetCompilerService
    {
        const string DotNetVersion = Extensions.DOTNET_SIX_VERSION;
        const string Language = Extensions.CSHARP_LANGUAGE;

        /// <summary>
        /// Gets the information about the target .NET version.
        /// </summary>
        public DotNetVersionInfo DotNetVersionInfo { get; }

        public CSharpCodeCompilerService(IDotNetFrameworkProvider dotNetFrameworkProvider)
        {
            DotNetVersionInfo = dotNetFrameworkProvider.GetDotNetVersion(DotNetVersion, Language);
        }

        /// <summary>
        /// Compiles the provided C# code into a dynamic assembly.
        /// </summary>
        public CompileOutput CompileText(string code)
        {
            try
            {
                var syntaxTree = BuildSyntaxTree(code);
                string assemblyName = Path.GetRandomFileName();
                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new List<SyntaxTree>() { syntaxTree },
                    references: GetMetadataReferences(DotNetVersionInfo.AvailableMetaData),
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithPlatform(Microsoft.CodeAnalysis.Platform.AnyCpu)
                    .WithUsings(DotNetVersionInfo.DefaultUsing));

                if (TryCompile(compilation, out Assembly? assembly, out string? errors))
                    return new CompileOutput { Assembly = assembly, CompileSucceeded = true, Errors = errors };
                else
                    return new CompileOutput { Errors = errors, CompileSucceeded = false };
            }
            catch (Exception ex)
            {
                return new CompileOutput { Errors = ex.Message, CompileSucceeded = false };
            }
        }

        /// <summary>
        /// Attempts to compile the provided C# compilation and loads the resulting assembly.
        /// </summary>
        bool TryCompile(CSharpCompilation compilation, out Assembly? assembly, out string? errors)
        {
            assembly = null;
            errors = null;
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    StringBuilder errorBuilder = new();
                    foreach (var item in failures)
                        errorBuilder.AppendLine(item.GetMessage());

                    errors = errorBuilder.ToString();
                    return false;
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    return true;
                }
            }
        }

        /// <summary>
        /// Converts a collection of file paths into metadata references for the compilation.
        /// </summary>
        IEnumerable<MetadataReference> GetMetadataReferences(IEnumerable<string> files)
        {
            List<MetadataReference> metadataReferences = new();
            foreach (var file in files)
                metadataReferences.Add(MetadataReference.CreateFromFile(file));

            return metadataReferences;
        }

        /// <summary>
        /// Builds a syntax tree from the provided C# code.
        /// </summary>
        SyntaxTree BuildSyntaxTree(string code) => CSharpSyntaxTree.ParseText(code);
    }
}
