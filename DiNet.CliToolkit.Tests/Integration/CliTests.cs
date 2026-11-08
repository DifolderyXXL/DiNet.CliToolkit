using DiNet.CliToolkit.Tests.Fixtures;

namespace DiNet.CliToolkit.Tests.Integration;

public class CliTests : IClassFixture<CliIntegrationFixture>
{
    private readonly CliIntegrationFixture _fixture;

    public CliTests(CliIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestHandler_Prints_Provided_Value()
    {
        // Act
        var output = await _fixture.RunCliAsync(["test", "--r-value", "TestValue"]);

        // Assert
        Assert.Contains("TestValue", output);
    }

    [Fact]
    public async Task HelpCommand_Should_Print_Available_Commands()
    {
        // Act
        var output = await _fixture.RunCliAsync(["help"]);

        // Assert
        Assert.DoesNotContain("nested", output);
        Assert.Contains("test", output);
        Assert.Contains("gen", output);
    }
}