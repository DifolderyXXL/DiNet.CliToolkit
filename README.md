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

---

## ⚙️ Installation

```bash
dotnet add package DiNet.CliToolkit