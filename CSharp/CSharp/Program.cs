﻿using Autofac;
using CSharp.Services;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var container = ConfigureContainer();
            
            var code1 = @"public class MainClass
                        {
                            public string Main()
                            {
                                return (2+3).ToString();
                            }
                        }";
            
            var guid= Guid.NewGuid();
            var dotNetCompilerService = container.Resolve<IDotNetCodeAnalyzerService>();
            var analyzeOutput = await dotNetCompilerService.AnalyzeCodeAsync(code1, 2000);
            Console.WriteLine(analyzeOutput.Success);
            Console.WriteLine(analyzeOutput.Errors);
            Console.WriteLine(analyzeOutput.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Environment.Exit(1);
        }
    }

    private static IContainer ConfigureContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<DotNetFrameworkProvider>().As<IDotNetFrameworkProvider>().SingleInstance();
        builder.RegisterType<CSharpCodeCompilerService>().As<IDotNetCompilerService>().SingleInstance();
        builder.RegisterType<CSharpCodeAnalyzerService>().As<IDotNetCodeAnalyzerService>().SingleInstance();
        return builder.Build();
    }
}