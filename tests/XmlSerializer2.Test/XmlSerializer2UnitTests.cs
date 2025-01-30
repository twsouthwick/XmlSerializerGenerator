using Microsoft.VisualStudio.TestTools.UnitTesting;

using Verify = XmlSerializer2.Test.SourceVerifier;

namespace XmlSerializer2.Test;

[TestClass]
public class XmlSerializer2UnitTest
{
    [TestMethod]
    public void SimplePoco()
    {
        Verify.Run(
            className: "Test",
            poco: """
              public class Test
              {
                  public int Value { get; set; }

                  public static Test Create() => new Test { Value = 5 };
              }
              """,
            expected: """"
              <?xml version="1.0" encoding="utf-16"?>
              <Test xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                <Value>5</Value>
              </Test>
              """"
              );
    }

    [TestMethod]
    public void CollectionType()
    {
        Verify.Run(
            className: "Employees",
            poco: """
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

                  public static Employees Create()
                  {
                      var John100 = new Employee("John", "100xxx");
                      var Emps = new Employees
                      {
                          // Note that only the collection is serialized -- not the
                          // CollectionName or any other public property of the class.
                          CollectionName = "Employees"
                      };
                      Emps.Add(John100);

                      return Emps;
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
              """,
            expected: """
              <?xml version="1.0" encoding="utf-16"?>
              <ArrayOfEmployee xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                <Employee>
                  <EmpName>John</EmpName>
                  <EmpID>100xxx</EmpID>
                </Employee>
              </ArrayOfEmployee>
              """
            );
    }
}

