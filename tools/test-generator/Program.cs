using System.Xml.Serialization;

Serializers.MyClass.Serialize(Console.Out, new MyClass { Value = 5 });

public class MyClass
{
    public int Value { get; set; }

    public int Value2 { get; set; }

    public int[] Values { get; set; } = null!;

    public List<string> Values2 { get; set; } = null!;

    public AccessibleRole Role { get; set; } = AccessibleRole.Alert;
}

[XmlSerializable(typeof(MyClass))]
partial class Serializers
{
}
