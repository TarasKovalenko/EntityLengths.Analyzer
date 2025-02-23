# EntityLengths.Analyzer

[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://taraskovalenko.github.io/)
[![build](https://github.com/TarasKovalenko/EntityLengths.Analyzer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/TarasKovalenko/EntityLengths.Analyzer/actions)
[![EntityLengths.Generator NuGet current](https://img.shields.io/nuget/v/EntityLengths.Analyzer?label=EntityLengths.Analyzer)](https://www.nuget.org/packages/EntityLengths.Analyzer/)

## Goals

A Roslyn analyzer that helps enforce string length constraints in your C# code. It analyzes property assignments and
verifies they comply with length restrictions defined through attributes or Entity Framework configurations.

## Terms of use

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all of the following statements:

- You unequivocally condemn Russia and its military aggression against Ukraine
- You recognize that Russia is an occupant that unlawfully invaded a sovereign state
- You agree that [Russia is a terrorist state](https://www.europarl.europa.eu/doceo/document/RC-9-2022-0482_EN.html)
- You fully support Ukraine's territorial integrity, including its claims
  over [temporarily occupied territories](https://en.wikipedia.org/wiki/Russian-occupied_territories_of_Ukraine)
- You reject false narratives perpetuated by Russian state propaganda

To learn more about the war and how you can help, [click here](https://war.ukraine.ua/). Glory to Ukraine! 🇺🇦

## Features

- Detects potential string length violations at compile time
- Supports multiple ways of defining length constraints:
    - EF Core Fluent API configurations (`HasMaxLength`)
    - Data Annotations
        - `[MaxLength]`
        - `[StringLength]`
    - Column type definitions
        - `[Column(TypeName = "varchar(200)")]`
        - `[Column(TypeName = "nvarchar(200)")]`
        - `[Column(TypeName = "char(200)")]`

## Installation

Install the library via NuGet Package Manager:

```bash
dotnet add package EntityLengths.Analyzer
```

## Usage

Using Attributes

```csharp
public class User
{
    [MaxLength(50)]
    public string Name { get; set; }  // Will show info/error if assigned string > 50 chars

    [StringLength(100)]
    public string Description { get; set; }  // Will show info/error if assigned string > 100 chars

    [Column(TypeName = "varchar(200)")]
    public string Url { get; set; }  // Will show info/error if assigned string > 200 chars
}
```

Using Entity Framework Fluent API

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(50);  // Will show warning/info if assigned string > 50 chars
    }
}

// Or in DbContext
public class UserDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(b => b.Name)
            .HasMaxLength(50);
    }
}
```

## Diagnostics

The analyzer provides two types of diagnostics:

- ML001 (Info): Warns about potential length violations when assigning non-literal values

```csharp
[MaxLength(5)]
public string Name { get; set; }

string value = GetValue();
Name = value;  // ML001: Property 'Name' has a maximum length of 5. Consider adding length validation.
```

- ML002 (Error): Reports definite length violations with string literals

```csharp
[MaxLength(5)]
public string Name { get; set; }

Name = "This is too long";  // ML002: String length (14) exceeds maximum length of 5 for property 'Name'
```
