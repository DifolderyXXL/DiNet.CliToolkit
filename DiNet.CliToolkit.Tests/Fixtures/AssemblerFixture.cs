using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Core.Models;
using DiNet.CliToolkit.Infrastructure;
using DiNet.CliToolkit.Infrastructure.Formatters;

namespace DiNet.CliToolkit.Tests.Fixtures;

public sealed class AssemblerFixture
{
    public INameFormatter Formatter { get; } = new DefaultNameFormatter();
    public IObjectAssembler CreateAssembler(IFlatArguments args)
    {
        var classifier = new CommandTypeClassifier();
        var deserializer = new ParameterDeserializer();
        var descriptorContext = new DescriptorContext(new ObjectDescriptorFactory());
        return new ObjectAssembler(
            descriptorContext,
            classifier,
            new ConstructorInvoker(deserializer, args, classifier),
            new PropertyInjector(deserializer, args, classifier));
    }

    public IFlatArguments Args(string? first = null, params ConsoleParameter[] ps)
        => new FlatArgumentArray(first, ps, Formatter);
}