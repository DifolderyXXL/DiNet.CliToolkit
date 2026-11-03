using DiNet.CliToolkit.Core.Attributes;

namespace DiNet.CliToolkit.Tests.Models;

public class Nested
{
    public int Value { get; set; }
    public string? Label { get; set; }
}


public class WithComplexParameter
{
    public Nested? Nested { get; }
    public WithComplexParameter(Nested? nested = null) => Nested = nested;
}

public class WithComplexProperty
{
    public Nested? Nested { get; set; }
}




public class PropertyWithConstructor
{
    public int Value { get; }
    public PropertyWithConstructor(int value)
    {
        Value = value;
    }
}
public class WithPropertyWithConstructor
{
    public PropertyWithConstructor? Prop { get; set; }
}