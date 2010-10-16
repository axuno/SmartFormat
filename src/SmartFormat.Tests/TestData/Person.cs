using System;
using System.ComponentModel;



namespace SmartFormat.Tests
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Person
    {
	    public Person()
	    {
	    }
	    public Person(string newName, System.DateTime newBirthday, string newAddress, params Person[] newFriends)
	    {
		    this.FullName = newName;
		    this.mBirthday = newBirthday;
		    if (!string.IsNullOrEmpty(newAddress))
			    this.mAddress = Address.Parse(newAddress);
		    this.mFriends = newFriends;
	    }

	    private string mFirst;
	    private string mLast;
	    private string mMiddle;
	    public string FirstName {
		    get { return mFirst; }
		    set { this.mFirst = value; }
	    }
	    public string LastName {
		    get { return mLast; }
		    set { this.mLast = value; }
	    }
	    public string MiddleName {
		    get { return mMiddle; }
		    set { this.mMiddle = value; }
	    }

	    public string FullName {
		    get {
			    if (string.IsNullOrEmpty(this.mMiddle)) {
				    return this.mFirst + " " + this.mLast;
			    } else {
				    return this.mFirst + " " + this.mMiddle + " " + this.mLast;
			    }
		    }
		    set {
			    string[] names = value.Split(' ');
			    this.mFirst = names[0];
			    if (names.Length == 2) {
				    this.mLast = names[1];
			    } else if (names.Length == 3) {
				    this.mMiddle = names[1];
				    this.mLast = names[2];
			    } else {
				    this.mMiddle = names[1];
				    this.mLast = string.Join(" ", names, 2, names.Length - 2);
			    }
		    }
	    }


	    private System.DateTime mBirthday;
	    public System.DateTime Birthday {
		    get { return mBirthday; }
		    set { mBirthday = value; }
	    }

	    private Address mAddress;
	    public Address Address {
		    get { return mAddress; }
		    set { mAddress = value; }
	    }


	    private Person[] mFriends = {
		
	    };
	    public Person[] Friends {
		    get { return mFriends; }
		    set { mFriends = value; }
	    }


	    public int NumberOfFriends {
		    get { return this.mFriends.Length; }
	    }


	    public int Age {
		    get {
			    if (Birthday.Month < DateTime.Now.Month || (Birthday.Month == DateTime.Now.Month && Birthday.Day <= DateTime.Now.Day)) {
				    return DateTime.Now.Year - Birthday.Year;
			    } else {
				    return DateTime.Now.Year - 1 - Birthday.Year;
			    }
		    }
	    }
	    public override string ToString()
	    {
		    return LastName + ", " + FirstName;
	    }
    }
}