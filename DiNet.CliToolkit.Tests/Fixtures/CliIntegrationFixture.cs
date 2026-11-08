using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Services;
using DiNet.CliToolkit.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiNet.CliToolkit.Tests.Fixtures;


public record TestObject(string RValue, int v = 5) : RootBaseQuery
{
    public Nested Nested { get; set; } = null;
    [Leaf]
    public GenericConvertable<string>? Gen { get; set; } = null;
}
public abstract record RootBaseQuery : IQuery<bool>
{
    [NotFlagged]
    public string? RootPath { get; set; } = null;
    public string? Ignore { get; set; } = null;
    [Leaf]
    public GenericConvertable<string>? OnlyExtensions { get; set; } = null;

    public string Root => RootPath ?? Environment.CurrentDirectory;
}


public class TestHandler : IQueryHandler<TestObject, bool>
{
    public async Task<bool> Handle(TestObject query, CancellationToken ct)
    {
        Console.WriteLine(query.RValue);
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
                .IntroduceHelpCommand("help")
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