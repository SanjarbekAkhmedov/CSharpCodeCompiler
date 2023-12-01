using System.Reflection;
using System.Text;

namespace CSharp.Services
{
    internal class CSharpCodeAnalyzerService : IDotNetCodeAnalyzerService
    {
        IDotNetCompilerService _dotNetCompilerService;
        public CSharpCodeAnalyzerService(IDotNetCompilerService dotNetCompilerService)
        {
            _dotNetCompilerService = dotNetCompilerService;
        }

        public Task<CodeAnalyzeOutput> AnalyzeCodeAsync(string code, int timeoutMilliseconds = default)
        {
            return Task.Run(() => AnalyzeCode(code, timeoutMilliseconds));
        }

        CodeAnalyzeOutput AnalyzeCode(string code, int timeoutMilliseconds)
        {
            bool succeed;
            var errorStringBuilder = new StringBuilder();
            var compileOutput = _dotNetCompilerService.CompileText(code);
            var result = string.Empty;

            if (compileOutput.CompileSucceeded)
            {
                succeed = true;
                var mainClass = GetMainClass(compileOutput.Assembly);
                var mainClassInstance = Activator.CreateInstance(mainClass);
                var mainMethod = GetMainMethodFromClassType(mainClass);

                var invokeResult = InvokeMethodWithTimeout(mainMethod, mainClassInstance, timeoutMilliseconds, out string assertionFailedError);

                if (!invokeResult.success)
                {
                    succeed = false;
                    errorStringBuilder.AppendLine(assertionFailedError);
                }

                result = invokeResult.result;
            }
            else
            {
                succeed = false;
                errorStringBuilder.AppendLine("Code was not compiled");
                errorStringBuilder.AppendLine(compileOutput.Errors);
            }

            return new CodeAnalyzeOutput { Errors = errorStringBuilder.ToString(), Success = succeed, Result = result };
        }

        (bool success, string result) InvokeMethodWithTimeout(MethodInfo testMethod, object classInstance, int timeoutMilliseconds, out string assertionFailedError)
        {
            assertionFailedError = string.Empty;

            try
            {
                var task = Task.Run(() => testMethod.Invoke(classInstance, null));
                if (task.Wait(timeoutMilliseconds))
                {
                    return (true, task.Result?.ToString() ?? string.Empty);
                }
                else
                {
                    assertionFailedError = "Execution timed out.";
                    return (false, string.Empty);
                }
            }
            catch (Exception ex)
            {
                assertionFailedError = ex.ToString();
                return (false, string.Empty);
            }
        }

        Type GetMainClass(Assembly assembly)
        {
            return assembly.GetTypes().First(s => s.Name == "MainClass");
        }

        static MethodInfo GetMainMethodFromClassType(Type classType)
        {
            return classType.GetMethods().First(method => method.Name == "Main");
        }
    }
}
