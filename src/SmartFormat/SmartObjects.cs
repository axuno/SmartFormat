using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFormat
{
    /// <summary>
    /// Represents a list of objects to be used as a value argument to <c>Smart.Format</c>/>.
    /// With SmartObjects
    /// * all objects used for Smart.Format can be collected in one place as the first argument
    /// * the format string can be written like each object would be the first argument of Smart.Format
    /// * there is no need to bother from which argument a value should come from
    /// </summary>
    /// <remarks>
    /// In case more than one object has the same member (or key) name, the value of the first object in the list will
    /// prevail.
    /// Change the order of objects in the list to change the object priority.
    /// </remarks>
    /// <code>
    /// var d1 = new Dictionary&lt;string,string&gt; { {"myKey", "myValue"} };
    /// var d2 = new Dictionary&lt;string,string&gt; { {"mySecondKey", "mySecondValue"} };
    /// var smartObj = new SmartObjects();
    /// smartObj.AddRange(new object[] {d1, d2});
    /// Smart.Format("{myKey} - {mySecondKey}", smartSrc);
    /// result: "myValue - mySecondValue"
    /// </code>
    [Obsolete("Depreciated in favor of ValueTuples", false)]
    public class SmartObjects : List<object>
    {
        /// <summary>
        /// Initializes a new instance of the <c>SmartObjects</c> class.
        /// </summary>
        public SmartObjects()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SmartObjects</c>
        /// class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="objList">The collection whose elements are copied to the new list.</param>
        public SmartObjects(IEnumerable<object> objList)
        {
            AddRange(objList);
        }

        /// <summary>
        /// Adds an object to the end of list.
        /// </summary>
        /// <param name="obj">Any object except types of SmartSource</param>
        public new void Add(object obj)
        {
            if (obj == null) throw new ArgumentNullException($"{nameof(obj)} must not be null.", nameof(obj));

            if (obj is SmartObjects)
                throw new ArgumentException($"Objects of type '{nameof(SmartObjects)}' cannot be nested.", nameof(obj));
            base.Add(obj);
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the list.
        /// </summary>
        /// <param name="objList">Any list of objects except objects of type SmartSource</param>
        public new void AddRange(IEnumerable<object> objList)
        {
            if (objList == null)
                throw new ArgumentNullException($"'{nameof(objList)}' must not be null.", nameof(objList));

            var objects = objList.ToArray();
            if (objects.Any(o => o is SmartObjects))
                throw new ArgumentException(
                    $"Objects of type '{nameof(SmartObjects)}' cannot be nested. At least one object in the argument list has type '{nameof(SmartObjects)}'.",
                    nameof(objList));
            base.AddRange(objects);
        }
    }
}