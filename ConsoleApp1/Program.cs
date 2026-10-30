Console.WriteLine(typeof(string).IsPrimitive);

var e = new Test();
Console.WriteLine(string.Join(", ", e.A));
Console.WriteLine(string.Join(", ", e.A));
Console.WriteLine(string.Join(", ", e.A));



class Test
{
    public string val { get; init; }
    public IEnumerable<int> A { get; }
    public Test()
    {
        var list = new List<int>() { 1 , 2, 3};
        A = list.Where(x =>
        {
            Console.WriteLine($"GO {x}");
            return x != 2;
        }).ToArray();
    }
}