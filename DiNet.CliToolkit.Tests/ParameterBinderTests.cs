using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Core.Models;
using DiNet.CliToolkit.Infrastructure;
using DiNet.CliToolkit.Infrastructure.Formatters;
using DiNet.CliToolkit.Infrastructure.TypeConverters;
using DiNet.CliToolkit.Services.Arguments;
using DiNet.CliToolkit.Tests;

namespace DiNet.CliToolkit.Tests;

public class PositionalHandler
{
    public int Id { get; }
    public string Name { get; }
    public bool Verbose { get; }
    public double? Ratio { get; }

    public PositionalHandler([NotFlagged] int id, string name, bool verbose = false, double? ratio = 1.5)
    {
        Id = id;
        Name = name;
        Verbose = verbose;
        Ratio = ratio;
    }
}

public class SampleHandler
{
    public int Id { get; }
    public string Name { get; }
    public bool Verbose { get; }
    public double? Ratio { get; }

    public SampleHandler(int id, string name, bool verbose = false, double? ratio = 1.5)
    {
        Id = id;
        Name = name;
        Verbose = verbose;
        Ratio = ratio;
    }
}


public class ObjectAssemblerTests
{
    private static readonly INameFormatter Formatter = new DefaultNameFormatter();

    private static IFlatArguments Args(string? firstUnflagged, params ConsoleParameter[] parameters)
        => new FlatArgumentArray(firstUnflagged, parameters, Formatter);

    [Fact]
    public void CreateRoot_PositionalHandler_AllParameters_ReturnsFilledObject()
    {
        var assembler = CreateAssembler(Args(
            "123",
            new ConsoleParameter("name", "Alice"),
            new ConsoleParameter("verbose", "true"),
            new ConsoleParameter("ratio", "2.5")));

        var result = (PositionalHandler)assembler.CreateRoot(typeof(PositionalHandler));

        Assert.Equal(123, result.Id);
        Assert.Equal("Alice", result.Name);
        Assert.True(result.Verbose);
        Assert.Equal(2.5, result.Ratio);
    }

    [Fact]
    public void CreateRoot_PositionalHandler_FlagWithoutValue_ReturnsTrue()
    {
        var assembler = CreateAssembler(Args(
            "123",
            new ConsoleParameter("name", "Alice"),
            new ConsoleParameter("verbose", null),
            new ConsoleParameter("ratio", "2.5")));

        var result = (PositionalHandler)assembler.CreateRoot(typeof(PositionalHandler));

        Assert.True(result.Verbose);
    }

    [Fact]
    public void CreateRoot_PositionalHandler_OptionalOmitted_UsesDefaults()
    {
        var assembler = CreateAssembler(Args(
            "123",
            new ConsoleParameter("name", "Alice")));

        var result = (PositionalHandler)assembler.CreateRoot(typeof(PositionalHandler));

        Assert.Equal(123, result.Id);
        Assert.Equal("Alice", result.Name);
        Assert.False(result.Verbose);
        Assert.Equal(1.5, result.Ratio);
    }

    [Fact]
    public void CreateRoot_SampleHandler_AllParameters_ReturnsFilledObject()
    {
        var assembler = CreateAssembler(Args(
            null,
            new ConsoleParameter("id", "123"),
            new ConsoleParameter("name", "Alice"),
            new ConsoleParameter("verbose", "true"),
            new ConsoleParameter("ratio", "2.5")));

        var result = (SampleHandler)assembler.CreateRoot(typeof(SampleHandler));

        Assert.Equal(123, result.Id);
        Assert.Equal("Alice", result.Name);
        Assert.True(result.Verbose);
        Assert.Equal(2.5, result.Ratio);
    }

    [Fact]
    public void CreateRoot_SampleHandler_MissingRequiredId_Throws()
    {
        var assembler = CreateAssembler(Args(
            null,
            new ConsoleParameter("Name", "Alice")));

        Assert.Throws<ArgumentException>(
            () => assembler.CreateRoot(typeof(SampleHandler)));
    }

    [Fact]
    public void CreateRoot_SampleHandler_UnflaggedArgIgnored()
    {
        var assembler = CreateAssembler(Args(
            "123",
            new ConsoleParameter("Name", "Alice")));

        Assert.Throws<ArgumentException>(
            () => assembler.CreateRoot(typeof(SampleHandler)));
    }

    private static IObjectAssembler CreateAssembler(IFlatArguments args)
    {
        var classifier = new CommandTypeClassifier();
        var deserializer = new ParameterDeserializer([new TypeConverterAdapter()]);
        var descriptorContext = new DescriptorContext(new ObjectDescriptorFactory());

        var injector = new PropertyInjector(deserializer, args, classifier);
        var invoker = new ConstructorInvoker(deserializer, args, classifier);

        return new ObjectAssembler(descriptorContext, classifier, invoker, injector);
    }
}