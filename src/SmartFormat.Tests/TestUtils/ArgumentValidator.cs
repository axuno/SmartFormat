using System;
using System.Collections;

namespace SmartFormat.Tests.Common
{
    public class ArgumentValidator
    {
        public static void CheckForNullReference(object? argument, string argumentName)
        {
            if (argument != null) return;
            throw new ArgumentNullException(argumentName);
        }

        public static void CheckForZeroValue(int argument, string argumentName)
        {
            if (argument != 0) return;
            throw new ArgumentOutOfRangeException(argumentName, argument, argumentName + " cannot be zero");
        }

        public static void CheckForEmpty(IEnumerable argument, string argumentName)
        {
            var enumerator = argument.GetEnumerator();
            if (enumerator.MoveNext()) return;
            throw new ArgumentException(argumentName + " cannot be empty", argumentName);
        }
    }
}
