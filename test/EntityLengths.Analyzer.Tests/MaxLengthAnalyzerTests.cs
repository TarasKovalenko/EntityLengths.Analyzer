using EntityLengths.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace EntityLengths.Analyzer.Tests;

public class MaxLengthAnalyzerTests
{
    [Fact]
    public async Task NoMaxLength_NoDiagnostic()
    {
        var test = """
                   public class TestClass
                   {
                       public string Name { get; set; }
                   
                       public void Test()
                       {
                           Name = "Test";
                       }
                   }
                   """;

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MaxLengthAttribute_AssignmentExceedsLimit_ReportsError()
    {
        var test = """
                   using System.ComponentModel.DataAnnotations;

                   public class TestClass
                   {
                       [MaxLength(5)]
                       public string Name { get; set; }
                   
                       public void Test()
                       {
                           Name = "Too Long Value";
                       }
                   }
                   """;

        var expected = DiagnosticResult
            .CompilerError("ML002")
            .WithSpan(10, 9, 10, 32)
            .WithArguments("14", "5", "Name")
            .WithMessage("String length (14) exceeds maximum length of 5 for property 'Name'");

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MaxLengthAttribute_NonLiteralAssignment_ReportsInfo()
    {
        var test = """
                   using System.ComponentModel.DataAnnotations;

                   public class TestClass
                   {
                       [MaxLength(5)]
                       public string Name { get; set; }
                   
                       public void Test()
                       {
                           var value = GetValue();
                           Name = value;
                       }
                   
                       private string GetValue() => "test";
                   }
                   """;

        var expected = new DiagnosticResult("ML001", DiagnosticSeverity.Info)
            .WithSpan(11, 9, 11, 21)
            .WithArguments("Name", "5")
            .WithMessage("Property 'Name' has a maximum length of 5. Consider adding length validation.");

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task StringLengthAttribute_AssignmentExceedsLimit_ReportsError()
    {
        var test = """
                   using System.ComponentModel.DataAnnotations;

                   public class TestClass
                   {
                       [StringLength(5)]
                       public string Name { get; set; }
                   
                       public void Test()
                       {
                           Name = "Too Long Value";
                       }
                   }
                   """;

        var expected = DiagnosticResult
            .CompilerError("ML002")
            .WithSpan(10, 9, 10, 32)
            .WithArguments("14", "5", "Name")
            .WithMessage("String length (14) exceeds maximum length of 5 for property 'Name'");

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ColumnAttribute_VarcharWithLength_ReportsError()
    {
        var test = """
                   using System.ComponentModel.DataAnnotations.Schema;

                   public class TestClass
                   {
                       [Column(TypeName = "varchar(10)")]
                       public string Name { get; set; }
                   
                       public void Test()
                       {
                           Name = "This is definitely too long for varchar(10)";
                       }
                   }
                   """;

        var expected = DiagnosticResult
            .CompilerError("ML002")
            .WithSpan(10, 9, 10, 61)
            .WithArguments("43", "10", "Name")
            .WithMessage("String length (43) exceeds maximum length of 10 for property 'Name'");

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task FluentApi_HasMaxLength_ReportsError()
    {
        var test = """
                   using Microsoft.EntityFrameworkCore;

                   public class User
                   {
                       public string Name { get; set; }
                   }

                   public class TestDbContext : DbContext
                   {
                       public DbSet<User> Users { get; set; }
                   
                       protected override void OnModelCreating(ModelBuilder modelBuilder)
                       {
                           modelBuilder.Entity<User>()
                               .Property(u => u.Name)
                               .HasMaxLength(20);
                       }
                   }

                   public class TestClass
                   {
                       public void Test(User user)
                       {
                           user.Name = "This is a very long string that exceeds twenty characters";
                       }
                   }
                   """;

        var expected = DiagnosticResult
            .CompilerError("ML002")
            .WithSpan(24, 9, 24, 80)
            .WithArguments("57", "20", "Name")
            .WithMessage("String length (57) exceeds maximum length of 20 for property 'Name'");

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task NonStringProperty_NoWarning()
    {
        var test = """
                   using System.ComponentModel.DataAnnotations;

                   public class TestClass
                   {
                       [MaxLength(5)]
                       public int Number { get; set; }
                   
                       public void Test()
                       {
                           Number = 12345;
                       }
                   }
                   """;

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleAttributes_UsesMaxLength()
    {
        var test = """
                   using System.ComponentModel.DataAnnotations;
                   using System.ComponentModel.DataAnnotations.Schema;

                   public class TestClass
                   {
                       [MaxLength(5)]
                       [StringLength(10)]
                       [Column(TypeName = "varchar(20)")]
                       public string Name { get; set; }
                   
                       public void Test()
                       {
                           Name = "Too Long Value";
                       }
                   }
                   """;

        var expected = DiagnosticResult
            .CompilerError("ML002")
            .WithSpan(13, 9, 13, 32)
            .WithArguments("14", "5", "Name")
            .WithMessage("String length (14) exceeds maximum length of 5 for property 'Name'");

        await CSharpAnalyzerVerifier<MaxLengthPropertyAnalyzer>
            .VerifyAnalyzerAsync(test, expected);
    }
}