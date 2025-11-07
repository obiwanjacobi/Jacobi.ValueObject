using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jacobi.ValueObject.Tests;

internal static class Generator
{
    private static GeneratorDriver Compile(string source, out Compilation compilation, out ImmutableArray<Diagnostic> diagnostics)
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
        return driver.RunGeneratorsAndUpdateCompilation(initialCompilation, out compilation, out diagnostics);
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

    public static Compilation Assert(string decl, string usage)
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
        Compile(sourceCode, out var compilation, out var diagnostics);
        Xunit.Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        Xunit.Assert.Empty(diagnostics);
        return compilation;
    }

    public static void AssertAndRun(string decl, string usage)
    {
        var compilation = Assert(decl, usage);
        Run(compilation);
    }

    public static void ExpectException<ExceptionT>(string decl, string usage)
    {
        try
        {
            var compilation = Assert(decl, usage);
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

    public static IEnumerable<Diagnostic> Errors(string source)
    {
        Compile(source, out var compilation, out var diagnostics);
        Xunit.Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        return diagnostics;
    }
}
