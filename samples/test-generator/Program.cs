using System.Xml.Serialization;

// Utilize the generated serializer
Serializers.MyClass.Serialize(Console.Out, new MyClass { Value = 5 });

// Get a serializer
Serializers.Get(typeof(MyClass)).Serialize(Console.Out, new MyClass { Value = 5 });

// POCO to be serialized
class MyClass
{
    public int Value { get; set; }

    public int Value2 { get; set; }

    public int[] Values { get; set; } = null!;

    public List<string> Values2 { get; set; } = null!;
}

// Let the generator know what type to serialize. It will generate a static member of
// type XmlSerializer within this class with the name of the supplied type
[XmlSerializable(typeof(MyClass))]
partial class Serializers
{
}
