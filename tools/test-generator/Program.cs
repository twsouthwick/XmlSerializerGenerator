﻿using System.Text.Json.Serialization;
using System.Xml.Serialization;

var serializer = new System.Xml.Serialization.XmlSerializer(typeof(MyClass));

var j = new XmlSerializersGenerated.MyClassSerializer();

j.Serialize(Console.Out, new MyClass { Value = 5 });

public class MyClass
{
    public int Value { get; set; }

    public int Value2 { get; set; }

    public int[] Values { get; set; } = null!;

    public List<string> Values2 { get; set; } = null!;

    public AccessibleRole Role { get; set; }
}


[JsonSerializable(typeof(MyClass))]
partial class T : JsonSerializerContext
{

}
