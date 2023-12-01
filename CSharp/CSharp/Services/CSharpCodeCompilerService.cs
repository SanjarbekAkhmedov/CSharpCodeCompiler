using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace CSharp.Services
{
    internal class CSharpCodeCompilerService : IDotNetCompilerService
    {
        const string DotNetVersion = Extensions.Extensions.DOTNET_SIX_VERSION;
        const string Language = Extensions.Extensions.CSHARP_LANGUAGE;
        public DotNetVersionInfo DotNetVersionInfo { get; }

        public CSharpCodeCompilerService(IDotNetFrameworkProvider dotNetFrameworkProvider)
        {
            DotNetVersionInfo = dotNetFrameworkProvider.GetDotNetVersion(DotNetVersion, Language);
        }

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
                    .WithPlatform(Platform.AnyCpu)
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
                    {
                        errorBuilder.AppendLine(item.GetMessage());
                    }
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

        IEnumerable<MetadataReference> GetMetadataReferences(IEnumerable<string> files)
        {
            List<MetadataReference> metadataReferences = new();
            foreach (var file in files)
            {
                string filePath = file;
                metadataReferences.Add(MetadataReference.CreateFromFile(filePath));
            }
            return metadataReferences;
        }

        SyntaxTree BuildSyntaxTree(string code)
        {
            return CSharpSyntaxTree.ParseText(code);
        }
    }
}
