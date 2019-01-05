using System;
using NUnit.Framework;
using SmartFormat.Net.Utilities;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ConditionalFormatterTests
    {
        private object[] GetArgs()
        {
            return new object[] {
                0,1,2,3,
                -1,-2, // {4},{5}
                TestFactory.GetPerson(), // {6}
                false,true, // {7},{8}
                // Note: only the date part will be compared:
                new DateTime(1111,1,1,1,1,1),SystemTime.Now(),new DateTime(5555,5,5,5,5,5), // {9},{10},{11}
                new TimeSpan(-1,-1,-1,-1,-1), TimeSpan.Zero,new TimeSpan(5,5,5,5,5), // {12},{13},{14}
                "Hello", "", // {15},{16}
                new {NotNull = true}, null, // {17},{18}
                // Note: only the date part will be compared:
                SystemTime.OffsetNow().AddDays(-1),SystemTime.OffsetNow(),SystemTime.OffsetNow().AddDays(1) // {19},{20},{21}
            };
        }

        [Test]
        public void Test_Numbers()
        {
            // Note: the "::" is necessary to bypass the PluralLocalizationExtension, and will be ignored in the output
            var formats = new[] {
                "{0::Zero|Other} {1::Zero|Other} {2::Zero|Other} {3::Zero|Other} {4::Zero|Other} {5::Zero|Other}",
                "{0::Zero|One|Other} {1::Zero|One|Other} {2::Zero|One|Other} {3::Zero|One|Other} {4::Zero|One|Other} {5::Zero|One|Other}",
                "{0::Zero|One|Two|Other} {1::Zero|One|Two|Other} {2::Zero|One|Two|Other} {3::Zero|One|Two|Other} {4::Zero|One|Two|Other} {5::Zero|One|Two|Other}",
            };
            var expected = new[] {
                "Zero Other Other Other Other Other",
                "Zero One Other Other Other Other",
                "Zero One Two Other Other Other",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_Enum()
        {
            var formats = new[] {
                "{6.Friends.0:{FirstName} is a {Gender:man|woman}.}",
                "{6.Friends.1:{FirstName} is a {Gender:man|woman}.}",
                "{9.DayOfWeek:Sunday|Monday|Some other day} / {11.DayOfWeek:Sunday|Monday|Some other day}",
            };
            var expected = new[] {
                "Jim is a man.",
                "Pam is a woman.",
                "Sunday / Some other day",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_Bool()
        {
            var formats = new[] {
                "{7:Yes|No}",
                "{8:Yes|No}",
            };
            var expected = new[] {
                "No",
                "Yes",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_Dates()
        {
            var formats = new[] {
                "{9:Past|Future} {10:Past|Future} {11:Past|Future}",
                "{9:Past|Present|Future} {10:Past|Present|Future} {11:Past|Present|Future}",
            };
            var expected = new[] {
                "Past Past Future",
                "Past Present Future",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_DateTimeOffset_Dates()
        {
            var formats = new[] {
                "{19:Past|Future} {20:Past|Future} {21:Past|Future}",
                "{19:Past|Present|Future} {20:Past|Present|Future} {21:Past|Present|Future}",
            };
            var expected = new[] {
                "Past Past Future",
                "Past Present Future",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_TimeSpan()
        {
            var formats = new[] {
                "{12:Past|Future} {13:Past|Future} {14:Past|Future}",
                "{12:Past|Zero|Future} {13:Past|Zero|Future} {14:Past|Zero|Future}",
            };
            var expected = new[] {
                "Past Past Future",
                "Past Zero Future",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_Strings()
        {
            var formats = new[] {
                "{15:{}|Empty} {16:{}|Empty} {18:{}|Null}",
            };
            var expected = new[] {
                "Hello Empty Null",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_Object()
        {
            var formats = new[] {
                "{17:{}|Null}; {18:{}|Null}",
            };
            var expected = new[] {
                "{ NotNull = True }; Null",
            };

            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_ComplexCondition()
        {
            var args = new object[] {-5, 0, 0.5, 1.0, 1.5, 5.0, 11.0M, 14.0f, 18, 22, 45, 60, 101};
            var formats = new[] {
                "{0::>0?Positive|<0?Negative|=0?Zero}, {1::>0?Positive|<0?Negative|=0?Zero}, {2::>0?Positive|<0?Negative|=0?Zero}",
                "{1::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{2::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{3::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{4::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{5::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{6::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{7::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{8::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{9::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{10::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{11::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
                "{12::<1?Baby|>=1&<4?Toddler|>=4&<=9?Child|=10/=11/=12?Pre-Teen|<18?Teenager|<20?Young Adult|<20/<=24&<25?Early Twenties|>55&<100?Senior Citizen|>100?Crazy Old|Adult}",
            };
            var expected = new[] {
                "Negative, Zero, Positive",
                "Baby",
                "Baby",
                "Toddler",
                "Toddler",
                "Child",
                "Pre-Teen",
                "Teenager",
                "Young Adult",
                "Early Twenties",
                "Adult",
                "Senior Citizen",
                "Crazy Old",
            };

            Smart.Default.Test(formats, args, expected);
        }

        [TestCase("{0:part(s)|car}", true, "part(s)")]
        [TestCase("{0:part(s)|car}", false, "car")]
        public void Syntax_should_not_be_confused_with_named_formatters(string format, object arg0, string expectedOutput)
        {
            var actualOutput = Smart.Format(format, arg0);
            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}
