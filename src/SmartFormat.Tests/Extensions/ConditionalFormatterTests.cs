using System;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ConditionalFormatterTests
    {
        [TestCase("{0:cond:Zero|Other} {1:cond:Zero|Other} {2:cond:Zero|Other} {3:cond:Zero|Other} {4:cond:Zero|Other} {5:cond:Zero|Other}", "Zero Other Other Other Other Other")]
        [TestCase("{0:cond:Zero|One|Other} {1:cond:Zero|One|Other} {2:cond:Zero|One|Other} {3:cond:Zero|One|Other} {4:cond:Zero|One|Other} {5:cond:Zero|One|Other}", "Zero One Other Other Other Other")]
        [TestCase("{0:cond:Zero|One|Two|Other} {1:cond:Zero|One|Two|Other} {2:cond:Zero|One|Two|Other} {3:cond:Zero|One|Two|Other} {4:cond:Zero|One|Two|Other} {5:cond:Zero|One|Two|Other}", "Zero One Two Other Other Other")]
        public void Test_Numbers(string format, string expected)
        {
            var args = new object[] {0, 1, 2, 3, -1, -2};
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0.Friends.0:{FirstName} is a {Gender:cond:man|woman}.}","Jim is a man.")]
        [TestCase( "{0.Friends.1:{FirstName} is a {Gender:cond:man|woman}.}","Pam is a woman.")]
        [TestCase("{2.DayOfWeek:cond:Sunday|Monday|Some other day} / {3.DayOfWeek:cond:Sunday|Monday|Some other day}","Sunday / Some other day")]
        public void Test_Enum(string format, string expected)
        {
            var args = new object[] {TestFactory.GetPerson(), TestFactory.GetPerson(), new DateTime(1111,1,1,1,1,1), new DateTime(5555,5,5,5,5,5)};
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0:cond:Yes|No}","No")]
        [TestCase("{1:cond:Yes|No}","Yes")]
        public void Test_Bool(string format, string expected)
        {
            var args = new object[]{false, true};
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [Test]
        public void Explicit_Formatter_With_Not_Enough_Parameters_Should_Throw()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.That(() => smart.Format("{0:cond:Yes}", 1), Throws.Exception.TypeOf<FormattingException>());
        }
        
        [TestCase("{0:cond:Past|Future} {1:cond:Past|Future} {2:cond:Past|Future}","Past Past Future")]
        [TestCase("{0:cond:Past|Present|Future} {1:cond:Past|Present|Future} {2:cond:Past|Present|Future}","Past Present Future")]
        public void Test_Dates(string format, string expected)
        {
            // only the date part will be compared
            var args = new object[] {new DateTime(1111,1,1,1,1,1),SystemTime.Now(),new DateTime(5555,5,5,5,5,5)};
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0:cond:Past|Future} {1:cond:Past|Future} {2:cond:Past|Future}", "Past Past Future")]
        [TestCase("{0:cond:Past|Present|Future} {1:cond:Past|Present|Future} {2:cond:Past|Present|Future}","Past Present Future")]
        public void Test_DateTimeOffset_Dates(string format, string expected)
        {
            // only the date part will be compared
            var args = new object[]
                {SystemTime.OffsetNow().AddDays(-1), SystemTime.OffsetNow(), SystemTime.OffsetNow().AddDays(1)};
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0:cond:Past|Future} {1:cond:Past|Future} {2:cond:Past|Future}", "Past Past Future")]
        [TestCase("{0:cond:Past|Zero|Future} {1:cond:Past|Zero|Future} {2:cond:Past|Zero|Future}","Past Zero Future")]
        public void Test_TimeSpan(string format, string expected)
        {
            var args = new object[] {new TimeSpan(-1,-1,-1,-1,-1), TimeSpan.Zero,new TimeSpan(5,5,5,5,5)};
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0:cond:{}|Empty}", "Hello")]
        [TestCase("{1:cond:{}|Empty}", "Empty")]
        [TestCase("{2:cond:{}|Null}", "Null")]
        public void Test_Strings(string format, string expected)
        {
            var args = new object[] { "Hello", "", null! };
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0:cond:{}|Null}", "{ NotNull = True }")] // 'expected' comes from the default formatter, writing the anonymous type with 'ToString()'
        [TestCase("{1:cond:{}|Null}", "Null")]
        public void Test_Object(string format, string expected)
        {
            var args = new object[] {new {NotNull = true}, null!};
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0::>0?Positive|<0?Negative|=0?Zero}, {1::>0?Positive|<0?Negative|=0?Zero}, {2::>0?Positive|<0?Negative|=0?Zero}", "Negative, Zero, Positive")]
        [TestCase("{1::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Baby")]
        [TestCase("{2::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Baby")]
        [TestCase("{3::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Toddler")]
        [TestCase("{4::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Toddler")]
        [TestCase("{5::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Child")]
        [TestCase("{6::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Pre-Teen")]
        [TestCase("{7::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Teenager")]
        [TestCase("{8::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Young Adult")]
        [TestCase("{9::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Early Twenties")]
        [TestCase("{10::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Adult")]
        [TestCase("{11::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Senior Citizen")]
        [TestCase("{12::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}", "Crazy Old")]
        public void Test_ComplexCondition(string format, string expected)
        {
            var args = new object[] {-5, 0, 0.5, 1.0, 1.5, 5.0, 11.0M, 14.0f, 18, 22, 45, 60, 101};

            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(format, args, expected);
        }

        [TestCase("{0:part(s)|car}", true, "part(s)")]
        [TestCase("{0:part(s)|car}", false, "car")]
        public void Syntax_should_not_be_confused_with_named_formatters(string format, object arg0, string expectedOutput)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var actualOutput = smart.Format(format, arg0);
            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}
