using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jacobi.ValueObject.Tests;

internal static class Generator
{
    private static string Compile(string source, out Compilation compilation, out ImmutableArray<Diagnostic> diagnostics)
    {
        var initialCompilation = CSharpCompilation.Create("Test",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
             MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll")),
             MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll")),
             MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(Jacobi.ValueObject.ValueObjectAttribute).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(Xunit.Assert).Assembly.Location),
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: false)
        );

        var generator = new Jacobi.ValueObject.Generator.Generator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var runDriver = driver.RunGeneratorsAndUpdateCompilation(initialCompilation, out compilation, out diagnostics);

        var builder = new StringBuilder();
        var result = runDriver.GetRunResult();
        foreach (var res in result.Results)
        {
            foreach (var gen in res.GeneratedSources)
            {
                builder.AppendLine(gen.SourceText.ToString());
            }
        }

        return builder.ToString();
    }

    private static void Run(Compilation compilation)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        Xunit.Assert.Empty(emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        stream.Position = 0;

        var loadCtx = new AssemblyLoadContext("test", isCollectible: true);
        try
        {
            var assembly = loadCtx.LoadFromStream(stream);

            Type? programType = assembly.GetType("Test.Program");
            MethodInfo? mainMethod = programType?.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            Xunit.Assert.NotNull(mainMethod);
            mainMethod.Invoke(null, null);
        }
        finally
        {
            loadCtx.Unload();

            // Wait for the unload to complete (optional, but helps in tests)
            for (int i = 0; i < 10 && GC.GetGeneration(loadCtx) >= 0; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }

    public static Compilation Assert(string decl, string usage, ITestOutputHelper? output = null)
    {
        var sourceCode = $$"""
            using System;
            using System.Collections.Generic;
            using Jacobi.ValueObject;
            using Xunit;
            namespace Test;
            {{decl}}
            public static class Program {
                public static void Main() {
                    {{usage}}
                }
            }
            """;
        var genSources = Compile(sourceCode, out var compilation, out var diagnostics);
        output?.WriteLine(genSources);

        Diagnostic[] diags = [.. diagnostics, .. compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)];
        if (output is not null)
        {
            foreach (Diagnostic diagnostic in diags)
            {
                output.WriteLine(diagnostic.ToString());
            }
        }
        Xunit.Assert.Empty(diags);
        //Xunit.Assert.Empty(diagnostics);
        return compilation;
    }

    public static void AssertAndRun(string decl, string usage, ITestOutputHelper? output = null)
    {
        var compilation = Assert(decl, usage, output);
        Run(compilation);
    }

    public static void ExpectException<ExceptionT>(string decl, string usage, ITestOutputHelper? output = null)
    {
        try
        {
            var compilation = Assert(decl, usage, output);
            Run(compilation);
        }
        catch (TargetInvocationException tie)
        {
            Xunit.Assert.IsType<ExceptionT>(tie.InnerException);
        }
        catch (Exception ex)
        {
            Xunit.Assert.IsType<ExceptionT>(ex);
        }
    }

    public static IEnumerable<Diagnostic> Errors(string source, ITestOutputHelper? output = null)
    {
        var genSources = Compile(source, out var compilation, out var diagnostics);
        if (output is not null)
            output.WriteLine(genSources);
        return [.. diagnostics, .. compilation.GetDiagnostics()];
    }
}
