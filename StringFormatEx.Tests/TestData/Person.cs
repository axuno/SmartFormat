using System.Collections.Generic;



public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public IList<Person> Friends { get; set; }
}