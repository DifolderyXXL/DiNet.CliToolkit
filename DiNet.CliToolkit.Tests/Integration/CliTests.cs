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
    public async Task HelpCommand_Should_Print_Available_Commands()
    {
        // Act
        var output = await _fixture.RunCliAsync(["test", "--value", "TestValue"]);

        // Assert
        Assert.Contains("TestValue", output);
    }

}