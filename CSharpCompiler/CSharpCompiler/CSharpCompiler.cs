using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace CSharpCompiler
{
    public static class CSharpCompiler
    {
        private const int ExecutionTimeout = 5000; // 5 seconds timeout
        public static async Task<string> CompileAndRunAsync(string code)
        {
            try
            {
                var references = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));
                var result = await ExecuteScriptAsync(code, references, new[] { "System", "System.Collections.Generic", "System.Linq" });

                if (result.Exception != null)
                    return result.Exception.Message;
                else
                    return result.ReturnValue?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private static async Task<ScriptState<object>> ExecuteScriptAsync(string code, IEnumerable<Assembly> references, IEnumerable<string> usings)
        {
            var options = ScriptOptions.Default.WithReferences(references).WithImports(usings);
            var task = Task.Run(() => CSharpScript.RunAsync(code, options));
            if (task.IsFaulted && task.Exception != null)
            {
                throw new Exception(task.Exception.Message);
            }
            if (await Task.WhenAny(task, Task.Delay(ExecutionTimeout)) == task)
                return await task;
            else
                throw new StackOverflowException("StackOverflow: Execution time exceeded.");
        }

        public static void CSharpCodeTests()
        {
            string code1 = @"2+3";

            string code2 = @"int a = 5;
                int b = 6;
                return a + b;";

            string code3 = @"int sum = 0;
                for (int i = 1; i <= 100; i++)
                    sum += i;
                return sum;";

            string code4 = @"int sum = 0;
                while (true)
                    sum++;
                return sum;";

            Console.WriteLine(CompileAndRunAsync(code1).GetAwaiter().GetResult());
            Console.WriteLine(CompileAndRunAsync(code2).GetAwaiter().GetResult());
            Console.WriteLine(CompileAndRunAsync(code3).GetAwaiter().GetResult());
            Console.WriteLine(CompileAndRunAsync(code4).GetAwaiter().GetResult());

            var student = new Student() { Id = 110, Name = "Sanjar" };
            string code = @"
                var dic = new Dictionary<string, string>()
                {
                    { ""Id"", @Id },
                    { ""Name"", @Name }
                };
                return dic.First().Key + "" "" + dic.First().Value;";

            string code5 = student.GetPayload(code);
            Console.WriteLine(CompileAndRunAsync(code5).GetAwaiter().GetResult());
            var dic = new Dictionary<string, string>();
        }
    }
}
