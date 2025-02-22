using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace EntityLengths.Analyzer.Tests.Verifiers;

public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test(source);
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }

    private sealed class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    {
        public Test(string source)
        {
            TestCode = source;
            TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net90;

            var metadataReferences = new[]
            {
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute)
                    .Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)
                    .Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly.Location)
            }.Distinct();

            foreach (var reference in metadataReferences)
            {
                TestState.AdditionalReferences.Add(reference);
            }
        }
    }
}