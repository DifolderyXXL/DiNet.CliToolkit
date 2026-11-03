using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Models;
using DiNet.CliToolkit.Tests.Fixtures;

namespace DiNet.CliToolkit.Tests.Assembly;


public class WithUnflaggedProperty
{
    [NotFlagged] public string? Name { get; set; }
    public int Count { get; set; }
}

public class WithUnflaggedConstructorParameter
{
    public WithUnflaggedConstructorParameter([NotFlagged] string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public class Node<T> 
{
    public T Value { get; set; }
}

public class SingleNested
{
    public Node<int> IntValue { get; set; }
}
public class DoubleNestedGenericOject
{
    public SingleNested Single { get; set; }
}

public class UnflaggedParameterTests(AssemblerFixture f) : IClassFixture<AssemblerFixture>
{

    [Fact]
    public void CreateRoot_UnflaggedProperty_UsesPositionalArg()
    {
        var assembler = f.CreateAssembler(f.Args("Bob", new ConsoleParameter("Count", "5")));
        var result = (WithUnflaggedProperty)assembler.CreateRoot(typeof(WithUnflaggedProperty));
        Assert.Equal("Bob", result.Name);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void CreateRoot_UnflaggedProperty_NoUnflaggedArg_StaysDefault()
    {
        var assembler = f.CreateAssembler(f.Args(null, new ConsoleParameter("Count", "5")));
        var result = (WithUnflaggedProperty)assembler.CreateRoot(typeof(WithUnflaggedProperty));
        Assert.Null(result.Name);
        Assert.Equal(5, result.Count);
    }


    [Fact]
    public void CreateRoot_UnflaggedProperty_NoUnflaggedArg_ButUsesPositionalArg()
    {
        var assembler = f.CreateAssembler(f.Args(null, new ConsoleParameter("Name", "Bob"), new ConsoleParameter("Count", "5")));
        var result = (WithUnflaggedProperty)assembler.CreateRoot(typeof(WithUnflaggedProperty));
        Assert.Equal("Bob", result.Name);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void CreateRoot_UnflaggedProperty_ConstructorParameter_ButUsesPositionalArg()
    {
        var assembler = f.CreateAssembler(f.Args(null, new ConsoleParameter("name", "Bob")));
        var result = (WithUnflaggedConstructorParameter)assembler.CreateRoot(typeof(WithUnflaggedConstructorParameter));
        Assert.Equal("Bob", result.Name);
    }

    [Fact]
    public void CreateRoot_UnflaggedProperty_ConstructorParameter_DoubleNestedGenericOject()
    {
        var assembler = f.CreateAssembler(f.Args(null, new ConsoleParameter("Value", "42")));
        var result = (DoubleNestedGenericOject)assembler.CreateRoot(typeof(DoubleNestedGenericOject));
        Assert.Equal(42, result.Single.IntValue.Value);
    }
}
