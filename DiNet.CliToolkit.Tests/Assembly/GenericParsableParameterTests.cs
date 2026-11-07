using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Tests.Fixtures;

namespace DiNet.CliToolkit.Tests.Assembly;


public class WithParsable
{
    [Leaf]
    public GenericConvertable<string>? Generic { get; set; } = null;
}

public class WithParsableInConstructor
{
    public GenericConvertable<string>? Generic { get; }

    public WithParsableInConstructor([Leaf] GenericConvertable<string>? Generic = null)
    {
        this.Generic = Generic;
    }
}
public class GenericParsableParameterTests(AssemblerFixture f) : IClassFixture<AssemblerFixture>
{
    [Theory]
    [InlineData(typeof(WithParsable))]
    [InlineData(typeof(WithParsableInConstructor))]
    public void WithoutParameterProvided(Type rootType)
    {
        var result = f.CreateAssembler(f.Args()).CreateRoot(rootType);
        var res = rootType.GetProperty("Generic")!.GetValue(result);
        Assert.Null(res);
    }

    [Theory]
    [InlineData(typeof(WithParsable))]
    [InlineData(typeof(WithParsableInConstructor))]
    public void WithParameterProvided(Type rootType)
    {
        var result = f.CreateAssembler(f.Args(null, [new("Generic","test")])).CreateRoot(rootType);
        var res = ((GenericConvertable<string>)rootType.GetProperty("Generic")!.GetValue(result)).Value;
        Assert.Equal("test", res);
    }
}

