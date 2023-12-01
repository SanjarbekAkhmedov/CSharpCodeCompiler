using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

namespace CSharpCompiler
{
    public static class JSCompiler
    {
        public static string ExecuteJavaScript(string code)
        {
            using (var engine = new V8JsEngine())
            {
                try
                {
                    var result = engine.Evaluate(code);
                    return result != null ? result.ToString() ?? "JavaScript result is null" : string.Empty;
                }
                catch (JsException ex)
                {
                    return $"JavaScript error: {ex.Message}";
                }
            }
        }

        public static void JSCodeTests()
        {
            string JavaScriptCode1 = "var x = 5 + 10; x;";
            string resultAsString1 = JSCompiler.ExecuteJavaScript(JavaScriptCode1);
            Console.WriteLine(resultAsString1);

            string JavaScriptCode2 = @"
                function Fibonacci(n) {
                    if (n <= 1) return n;
                    return Fibonacci(n - 1) + Fibonacci(n - 2);
                }
                Fibonacci(5);";
            string resultAsString2 = JSCompiler.ExecuteJavaScript(JavaScriptCode2);
            Console.WriteLine($"Result: {resultAsString2}");

            string JavaScriptCode3 = @"
                var sum = 0;
                function stackOverflowSimulator() {
                    sum = sum+1;
                    stackOverflowSimulator();
                }
                stackOverflowSimulator();";
            string resultAsString3 = JSCompiler.ExecuteJavaScript(JavaScriptCode3);
            Console.WriteLine($"Result: {resultAsString3}");
        }
    }
}
