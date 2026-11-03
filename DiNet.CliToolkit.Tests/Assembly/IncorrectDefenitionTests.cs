using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Models;
using DiNet.CliToolkit.Tests.Fixtures;
using DiNet.CliToolkit.Tests.Models;

namespace DiNet.CliToolkit.Tests.Assembly;

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


public class IncorrectDefenitionTests(AssemblerFixture f) : IClassFixture<AssemblerFixture>
{
    [Fact]
    public void CreateRoot_UnflaggedComplexProperty_Throws()
    {
        var assembler = f.CreateAssembler(f.Args("x"));
        Assert.Throws<IncorrectCommandDefenitionException>(
            () => assembler.CreateRoot(typeof(WithUnflaggedComplexProperty)));
    }

    [Fact]
    public void CreateRoot_UnflaggedComplexParameter_Throws()
    {
        var assembler = f.CreateAssembler(f.Args("x"));
        Assert.Throws<IncorrectCommandDefenitionException>(
            () => assembler.CreateRoot(typeof(WithUnflaggedComplexParameter)));
    }

    [Fact]
    public void CreateRoot_MisplacedNotFlagged_Throws()
    {
        var assembler = f.CreateAssembler(f.Args("x", new ConsoleParameter("id", "1")));
        Assert.Throws<IncorrectCommandDefenitionException>(
            () => assembler.CreateRoot(typeof(WithMisplacedNotFlagged)));
    }
}
