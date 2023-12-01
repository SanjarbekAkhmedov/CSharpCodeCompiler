namespace General.Automation.Subscriptions.WebHooks.CSharpCompiler.Services
{
    using General.Automation.Subscriptions.WebHooks.CSharpCompiler.Records;
    using System.Reflection;
    using System.Text;

    public interface IDotNetCodeAnalyzerService
    {
        /// <summary>
        /// Analyzes the provided C# code asynchronously, using the specified timeout.
        /// </summary>
        Task<CodeAnalyzeOutput> AnalyzeAndRunCodeAsync(string code, int timeoutMilliseconds = 10000);
    }

    public class CSharpCodeAnalyzerService : IDotNetCodeAnalyzerService
    {
        IDotNetCompilerService _dotNetCompilerService;
        public CSharpCodeAnalyzerService(IDotNetCompilerService dotNetCompilerService)
        {
            _dotNetCompilerService = dotNetCompilerService;
        }

        /// <summary>
        /// Analyzes the provided C# code asynchronously, using the specified timeout.
        /// </summary>
        public Task<CodeAnalyzeOutput> AnalyzeAndRunCodeAsync(string code, int timeoutMilliseconds = default)
        {
            return Task.Run(() => AnalyzeCode(code, timeoutMilliseconds));
        }

        /// <summary>
        /// Performs the actual code analysis, compiling and executing the provided C# code.
        /// </summary>
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

        /// <summary>
        /// Invokes a method with a specified timeout.
        /// </summary>
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

        /// <summary>
        /// Gets the main class type from the provided assembly.
        /// </summary>
        Type GetMainClass(Assembly assembly) => assembly.GetTypes().First(s => s.Name == "MainClass");

        /// <summary>
        /// Gets the main method from the provided class type.
        /// </summary>
        static MethodInfo GetMainMethodFromClassType(Type classType) => classType.GetMethods().First(method => method.Name == "Main");
    }
}
