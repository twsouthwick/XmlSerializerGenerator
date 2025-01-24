using System.Xml.Serialization;

var importer = new XmlReflectionImporter();
var mapping = importer.ImportTypeMapping(typeof(MyClass));

var serializer = new System.Xml.Serialization.XmlSerializer(typeof(MyClass));

public class MyClass
{
    public int Value { get; set; }
}
