using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Tests.Fixtures;

namespace DiNet.CliToolkit.Tests.Assembly;

public abstract record BaseClass
{
    [NotFlagged]
    public string? Value { get; set; }
}
public record ChildClass : BaseClass;


public class BaseClassTests(AssemblerFixture f) : IClassFixture<AssemblerFixture>
{
    [Theory]
    [InlineData(typeof(ChildClass))]
    public void WithoutArgs_NestedIsNull(Type rootType)
    {
        var msg = "Test";
        var result = f.CreateAssembler(f.Args(msg)).CreateRoot(rootType);
        var res = rootType.GetProperty("Value")!.GetValue(result);
        Assert.Equal(msg, res);
    }
}

