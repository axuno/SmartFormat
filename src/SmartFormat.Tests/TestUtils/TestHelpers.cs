using System;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;

namespace SmartFormat.Tests.TestUtils;

[DebuggerNonUserCode]
public static class TestHelpers
{
    public static void Test(this SmartFormatter formatter, string format, object[] args, string expected)
    {

        string? actual = null;
        try
        {
            actual = formatter.Format(format, args);
            Assert.AreEqual(expected, actual);
            Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: \"{0}\" => \"{1}\" - {2}", format, actual, ex.Message);
            throw;
        }
    }

    public static void Test(this SmartFormatter formatter, string format, object[][] bunchOfArgs, string[] bunchOfExpected)
    {
        var allErrors = new ExceptionCollection(); // We will defer all errors until the end.

        var numberOfTests = Math.Max(bunchOfArgs.Length, bunchOfExpected.Length);
        for (int i = 0; i < numberOfTests; i++)
        {
            var args = bunchOfArgs[i%bunchOfArgs.Length];
            var expected = bunchOfExpected[i%bunchOfExpected.Length];

            string? actual = null;
            try
            {
                actual = formatter.Format(format, args);
                Assert.AreEqual(expected, actual);
                Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: \"{0}\" => \"{1}\"", format, actual);
                allErrors.Add(ex);
            }
        }

        allErrors.ThrowIfNotEmpty();
    }

    public static void Test(this SmartFormatter formatter, string[] bunchOfFormat, object[] args, string[] bunchOfExpected)
    {
        var allErrors = new ExceptionCollection();

        var numberOfTests = Math.Max(bunchOfFormat.Length, bunchOfExpected.Length);
        for (int i = 0; i < numberOfTests; i++)
        {
            var format = bunchOfFormat[i%bunchOfFormat.Length];
            var expected = bunchOfExpected[i%bunchOfExpected.Length];

            string? actual = null;
            try
            {
                var specificCulture = new CultureInfo("en-us");
                actual = formatter.Format(specificCulture, format, args);
                Assert.AreEqual(expected, actual);
                Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: \"{0}\" => \"{1}\"", format, actual);
                allErrors.Add(ex);
            }
        }

        allErrors.ThrowIfNotEmpty();
    }
}