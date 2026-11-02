using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Core.Models;
using DiNet.CliToolkit.Infrastructure;
using DiNet.CliToolkit.Infrastructure.Formatters;

namespace DiNet.CliToolkit.Tests;

public class Nested
{
    public int Value { get; set; }
    public string? Label { get; set; }
}

public class WithComplexParameter
{
    public Nested? Nested { get; }
    public WithComplexParameter(Nested? nested = null) => Nested = nested;
}

public class WithComplexProperty
{
    public Nested? Nested { get; set; }
}

public class WithUnflaggedProperty
{
    [NotFlagged] public string? Name { get; set; }
    public int Count { get; set; }
}

public class WithUnflaggedComplexProperty
{
    [NotFlagged] public Nested? Nested { get; set; }
}

public class WithUnflaggedComplexParameter
{
    public WithUnflaggedComplexParameter([NotFlagged] Nested nested) { }
}

public class WithMisplacedNotFlagged
{
    public WithMisplacedNotFlagged(int id, [NotFlagged] string name) { }
}

public class PropertyWithConstructor
{
    public int Value { get; }
    public PropertyWithConstructor(int value)
    {
        Value = value;
    }
}
public class WithPropertyWithConstructor
{
    public PropertyWithConstructor? Prop { get; set; }
}

public class NestedObjectsTests
{
    private static readonly INameFormatter Formatter = new DefaultNameFormatter();

    [Fact]
    public void CreateRoot_ComplexConstructorParameter_NestedWithConstructor_Should_Not_Created()
    {
        var assembler = CreateAssembler(Args(
            null));

        var result = (WithPropertyWithConstructor)assembler.CreateRoot(typeof(WithPropertyWithConstructor));

        Assert.Null(result.Prop);
    }


    [Fact]
    public void CreateRoot_ComplexConstructorParameter_CreatesNested()
    {
        var assembler = CreateAssembler(Args(
            null,
            new ConsoleParameter("Value", "42"),
            new ConsoleParameter("Label", "hi")));

        var result = (WithComplexParameter)assembler.CreateRoot(typeof(WithComplexParameter));

        Assert.NotNull(result.Nested);
        Assert.Equal(42, result.Nested!.Value);
        Assert.Equal("hi", result.Nested.Label);
    }

    [Fact]
    public void CreateRoot_ComplexConstructorParameter_NoArg_UsesDefault()
    {
        var assembler = CreateAssembler(Args(null));
        var result = (WithComplexParameter)assembler.CreateRoot(typeof(WithComplexParameter));
        Assert.Null(result.Nested);
    }

    [Fact]
    public void CreateRoot_ComplexProperty_CreatesNested()
    {
        var assembler = CreateAssembler(Args(
            null,
            new ConsoleParameter("Value", "42")));

        var result = (WithComplexProperty)assembler.CreateRoot(typeof(WithComplexProperty));

        Assert.NotNull(result.Nested);
        Assert.Equal(42, result.Nested!.Value);
    }


    [Fact]
    public void Diagnostic_Nested_Descriptor_Resolves_Parameterless_Constructor()
    {
        var context = new DescriptorContext(new ObjectDescriptorFactory());
        var raw = context.GetOrGenerateObjectDescriptor(typeof(Nested));

        Assert.NotNull(raw.SingleConstructor);
        Assert.NotNull(raw.SingleConstructor.Constructor);

        var ctor = raw.SingleConstructor.Constructor;
        Assert.Equal(typeof(Nested), ctor.DeclaringType);
        Assert.Empty(ctor.GetParameters());
        Assert.Empty(raw.SingleConstructor.Parameters);
    }

    [Fact]
    public void Diagnostic_Nested_Descriptor_From_Root_Still_Parameterless()
    {
        var context = new DescriptorContext(new ObjectDescriptorFactory());
        var classifier = new CommandTypeClassifier();
        var raw = context.GetOrGenerateObjectDescriptor(typeof(Nested));
        var root = new RootObjectDescriptor(raw, classifier);

        Assert.Equal(typeof(Nested), root.SingleConstructor.Constructor.DeclaringType);
        Assert.Empty(root.SingleConstructor.Constructor.GetParameters());
        Assert.Empty(root.SingleConstructor.Parameters);
        Assert.Null(root.SingleConstructor.UnflaggedParameter);
    }


    [Fact]
    public void CreateRoot_ComplexProperty_NoArg_Skipped()
    {
        var assembler = CreateAssembler(Args(null));
        var result = (WithComplexProperty)assembler.CreateRoot(typeof(WithComplexProperty));
        Assert.Null(result.Nested);
    }

    [Fact]
    public void CreateRoot_UnflaggedProperty_UsesPositionalArg()
    {
        var assembler = CreateAssembler(Args("Bob", new ConsoleParameter("Count", "5")));
        var result = (WithUnflaggedProperty)assembler.CreateRoot(typeof(WithUnflaggedProperty));
        Assert.Equal("Bob", result.Name);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void CreateRoot_UnflaggedProperty_NoUnflaggedArg_StaysDefault()
    {
        var assembler = CreateAssembler(Args(null, new ConsoleParameter("Count", "5")));
        var result = (WithUnflaggedProperty)assembler.CreateRoot(typeof(WithUnflaggedProperty));
        Assert.Null(result.Name);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void CreateRoot_UnflaggedComplexProperty_Throws()
    {
        var assembler = CreateAssembler(Args("x"));
        Assert.Throws<IncorrectCommandDefenitionException>(
            () => assembler.CreateRoot(typeof(WithUnflaggedComplexProperty)));
    }

    [Fact]
    public void CreateRoot_UnflaggedComplexParameter_Throws()
    {
        var assembler = CreateAssembler(Args("x"));
        Assert.Throws<IncorrectCommandDefenitionException>(
            () => assembler.CreateRoot(typeof(WithUnflaggedComplexParameter)));
    }

    [Fact]
    public void CreateRoot_MisplacedNotFlagged_Throws()
    {
        var assembler = CreateAssembler(Args("x", new ConsoleParameter("id", "1")));
        Assert.Throws<IncorrectCommandDefenitionException>(
            () => assembler.CreateRoot(typeof(WithMisplacedNotFlagged)));
    }

    // ── Хелперы ──────────────────────────────────────────────────

    private static IFlatArguments Args(string? firstUnflagged, params ConsoleParameter[] parameters)
        => new FlatArgumentArray(firstUnflagged, parameters, Formatter);

    private static IObjectAssembler CreateAssembler(IFlatArguments args)
    {
        var classifier = new CommandTypeClassifier();
        var deserializer = new ParameterDeserializer();
        var descriptorContext = new DescriptorContext(new ObjectDescriptorFactory());

        var injector = new PropertyInjector(deserializer, args, classifier);
        var invoker = new ConstructorInvoker(deserializer, args, classifier);

        return new ObjectAssembler(descriptorContext, classifier, invoker, injector);
    }
}

