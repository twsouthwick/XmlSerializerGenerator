using System.Collections;
using System.Xml.Serialization;

// My class - with WinForms
Serializers.MyClass.Serialize(Console.Out, new MyClass { Value = 5 });

Console.WriteLine();

// Employee - collection
var John100 = new Employee("John", "100xxx");
var Emps = new Employees
{
    // Note that only the collection is serialized -- not the
    // CollectionName or any other public property of the class.
    CollectionName = "Employees"
};
Emps.Add(John100);

Test.Employees.Serialize(Console.Out, Emps);

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

[XmlSerializable(typeof(Employees))]
partial class Test
{
}

public class Employees : ICollection
{
    public string? CollectionName;
    private ArrayList empArray = new ArrayList();

    public Employee? this[int index] => (Employee?)empArray[index];

    public void CopyTo(Array a, int index)
    {
        empArray.CopyTo(a, index);
    }

    public int Count => empArray.Count;

    public object SyncRoot => this;

    public bool IsSynchronized => false;

    public IEnumerator GetEnumerator() => empArray.GetEnumerator();

    public void Add(Employee newEmployee)
    {
        empArray.Add(newEmployee);
    }
}

public class Employee
{
    public string? EmpName;
    public string? EmpID;

    public Employee() { }

    public Employee(string empName, string empID)
    {
        EmpName = empName;
        EmpID = empID;
    }
}
