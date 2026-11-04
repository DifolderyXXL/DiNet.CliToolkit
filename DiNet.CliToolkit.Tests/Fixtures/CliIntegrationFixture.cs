using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiNet.CliToolkit.Tests.Fixtures;


public record TestObject(string Value) : IQuery<bool>;
public class TestHandler : IQueryHandler<TestObject, bool>
{
    public async Task<bool> Handle(TestObject query, CancellationToken ct)
    {
        Console.WriteLine(query.Value);
        return true;
    }
}

public class CliIntegrationFixture
{
    public async Task<string> RunCliAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddCliToolkit(cli =>
        {
            cli.UseKebabCaseFormatter();

            cli.Handlers
                .Register<TestHandler>("test");
        });


        using var host = builder.Build();
        var cliApp = host.Services.GetRequiredService<CliApplication>();

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            await cliApp.Route(args, CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return stringWriter.ToString();
    }
}