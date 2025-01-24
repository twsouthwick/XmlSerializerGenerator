using System.Xml.Serialization;

var serializer = new System.Xml.Serialization.XmlSerializer(typeof(MyClass));

var j = new XmlSerializersGenerated.MyClassSerializer();

j.Serialize(Console.Out, new MyClass { Value = 5 });

public class MyClass
{
    public int Value { get; set; }
}


