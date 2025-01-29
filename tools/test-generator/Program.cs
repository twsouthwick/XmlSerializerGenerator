using System.Collections;

foreach (var data in typeof(Employees).GetCustomAttributesData())
{
    Console.WriteLine(data.AttributeType.ToString());
}

[Repro.MyAttr(typeof(Employees))]
public class Test
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
