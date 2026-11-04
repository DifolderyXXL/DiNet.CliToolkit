# DiNet.CliToolkit

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet Version](https://img.shields.io/nuget/v/DiNet.CliToolkit.svg)](https://www.nuget.org/packages/DiNet.CliToolkit/)

> A lightweight, reflection-based CLI routing framework with native Dependency Injection support for .NET.

---

## 📌 Overview

**DiNet.CliToolkit** is a custom reflection-driven routing engine built specifically for single-execution CLI applications. It allows you to build robust command-line interfaces by mapping command-line arguments directly to strongly-typed records, while leveraging the full power of `Microsoft.Extensions.DependencyInjection`.

Handlers are loosely coupled, easily testable, and bound via route words. You get the simplicity of a CLI with the architectural integrity of a modern .NET application.

---

## 🚀 Key Features

* **Native DI Support**: Handlers are resolved from the DI container, allowing seamless integration of your services (e.g., database connections, file processors, loggers).
* **Strongly-Typed Queries**: Commands are structured as records implementing `IQuery<T>`. Arguments are automatically parsed and matched.
* **Kebab-Case Formatting**: Built-in support for converting C# PascalCase properties to standard CLI kebab-case flags (e.g., `--shingle-size`).
* **Positional Arguments**: Use the `[NotFlagged]` attribute to pass primary arguments (like file paths) without explicit flag switches.
* **Boolean Switch Parsing**: Flags without explicit value bindings (e.g., `--verbose`) automatically evaluate to `true` when supplied, and `false` when omitted.
* **Nested Objects & Command Reuse**: Command records can embed nested strongly-typed objects, letting you extract shared parameter groups and reuse them across multiple commands.
* **Extensible Type Conversion**: Any type implementing `IParsable<T>` is supported out of the box, and you can plug in your own converters by registering an `IValueConverter` in DI.

---

## How to use

### 1. Define your Query
Create a strongly typed `IQuery<TResponse>` to retrieve parameters from args.
If you define a default value like `int ShingleSize = 4`, the value becomes optional. Otherwise, it is required and the command won't run if you don't pass that parameter.

```csharp
public record MyHashQuery(
    [NotFlagged] string? RootPath = null,
    int ShingleSize = 4,
    int NumHashes = 128,
    int ChunkStep = 16,
    uint Seed = 123456789,
    int Top = 5,
    string? Ignore = null) : IQuery<bool>;
```

### 1.1. Nested Objects (Command Reuse)
Group common parameters into a nested record and reuse it across several commands — arguments bind to it as if they were declared flat on the root query.

```csharp
public record Nested(int Value, string Label);

public record WithComplexParameter(Nested? Nested = null) : IQuery<bool>;

// CLI:  with-complex-parameter --value 42 --label Hello
//       -> Nested.Value == 42, Nested.Label == "Hello"
```

### 1.2. Custom Converters
Types implementing `IParsable<T>` (e.g. `GenericConvertable<int>` below) are parsed automatically. For arbitrary types, implement `IValueConverter` and register it in DI — the toolkit picks it up via constructor injection.

```csharp
public class GenericConvertable<T> : IParsable<GenericConvertable<T>> where T : IParsable<T>
{
    public T Value { get; }
    public GenericConvertable(T value) => Value = value;

    public static GenericConvertable<T> Parse(string s, IFormatProvider? p) => new(T.Parse(s, p));

    public static bool TryParse(string? s, IFormatProvider? p, out GenericConvertable<T>? r)
    {
        if (T.TryParse(s, p, out var v)) { r = new(v); return true; }
        r = null; return false;
    }
}

// Or a fully custom converter
public class MyConverter : IValueConverter
{
    public bool CanConvert(Type targetType) {...};
    public bool TryConvert(string value, Type targetType, out object? result) {...};
}

builder.Services.AddSingleton<IValueConverter, MyConverter>();
```

### 2. Implement your Handler
Implement a handler as `IQueryHandler<MyHashQuery, bool>` for your query.

```csharp
class MyHashQueryHandler(IDocumentMatchService matchService) : IQueryHandler<MyHashQuery, bool>
{
    public async Task<bool> Handle(MyHashQuery query, CancellationToken ct)
    {
        var context = await matchService.ExtractMatchGroupsAsync(
            query.RootPath, query.ShingleSize, query.NumHashes,
            query.ChunkStep, query.Seed, query.Ignore, ct);

        // ... some code

        foreach (var score in topScores)
        {
            var fileA = context.Store.GetDocument(score.FileA)?.FullPath ?? "Unknown";
            var fileB = context.Store.GetDocument(score.FileB)?.FullPath ?? "Unknown";
            Console.WriteLine($"{fileA} <-> {fileB} : {score.Similarity * 100:F2}%");
        }

        return true;
    }
}
```

### 3. Setup `Program.cs`
Setup your `Program.cs` to define services and handlers for the CLI.

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IDocumentMatchService, DocumentMatchService>(); // Add required services

builder.Services.AddCliToolkit(cli =>
{
    cli.UseKebabCaseFormatter(); // Define formatter for arguments

    cli.Handlers
        .IntroduceHelpCommand("help") // Optional help command, shows each available command with parameters
        .Register<MyHashQueryHandler>("hashquery"); // Register our handler
});

using var host = builder.Build();

// Next lines are the required code to handle our cli commands
var cliApp = host.Services.GetRequiredService<CliApplication>();
await cliApp.Route(args, CancellationToken.None);
```

---

## ⚙️ Installation

```bash
dotnet add package DiNet.CliToolkit
```