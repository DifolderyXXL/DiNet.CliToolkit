using DiNet.CliToolkit.Core.Models;
using DiNet.CliToolkit.Tests.Fixtures;
using DiNet.CliToolkit.Tests.Models;

namespace DiNet.CliToolkit.Tests.Assembly;


public class NestedCreationTests(AssemblerFixture f) : IClassFixture<AssemblerFixture>
{
    public static TheoryData<Type> ComplexRootTypes =>
    [
        typeof(WithComplexParameter),
        typeof(WithComplexProperty),
    ];

    [Theory]
    [MemberData(nameof(ComplexRootTypes))]
    public void WithoutArgs_NestedIsNull(Type rootType)
    {
        var result = f.CreateAssembler(f.Args()).CreateRoot(rootType);
        var nested = rootType.GetProperty("Nested")!.GetValue(result) as Nested;
        Assert.Null(nested);
    }

    [Theory]
    [MemberData(nameof(ComplexRootTypes))]
    public void WithArgs_NestedIsPopulated(Type rootType)
    {
        var p1 = new ConsoleParameter("Value", "42");
        var p2 = new ConsoleParameter("Label", "Hello");

        var result = f.CreateAssembler(
                f.Args(null, p1, p2))
            .CreateRoot(rootType);
        var nested = (Nested)rootType.GetProperty("Nested")!.GetValue(result)!;
        Assert.Equal(42, nested.Value);
        Assert.Equal("Hello", nested.Label);
    }
}

