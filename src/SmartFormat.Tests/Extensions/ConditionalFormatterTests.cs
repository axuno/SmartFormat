using System;
using NUnit.Framework;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ConditionalFormatterTests
    {
        [Test]
        public void Test_Numbers()
        {
            var formats = new[] {
                "{0:cond:Zero|Other} {1:cond:Zero|Other} {2:cond:Zero|Other} {3:cond:Zero|Other} {4:cond:Zero|Other} {5:cond:Zero|Other}",
                "{0:cond:Zero|One|Other} {1:cond:Zero|One|Other} {2:cond:Zero|One|Other} {3:cond:Zero|One|Other} {4:cond:Zero|One|Other} {5:cond:Zero|One|Other}",
                "{0:cond:Zero|One|Two|Other} {1:cond:Zero|One|Two|Other} {2:cond:Zero|One|Two|Other} {3:cond:Zero|One|Two|Other} {4:cond:Zero|One|Two|Other} {5:cond:Zero|One|Two|Other}",
            };
            var expected = new[] {
                "Zero Other Other Other Other Other",
                "Zero One Other Other Other Other",
                "Zero One Two Other Other Other",
            };

            var args = new object[] {0, 1, 2, 3, -1, -2};
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_Enum()
        {
            var formats = new[] {
                "{0.Friends.0:{FirstName} is a {Gender:man|woman}.}",
                "{0.Friends.1:{FirstName} is a {Gender:man|woman}.}",
                "{2.DayOfWeek:Sunday|Monday|Some other day} / {3.DayOfWeek:Sunday|Monday|Some other day}",
            };
            var expected = new[] {
                "Jim is a man.",
                "Pam is a woman.",
                "Sunday / Some other day",
            };

            var args = new object[] {TestFactory.GetPerson(), TestFactory.GetPerson(), new DateTime(1111,1,1,1,1,1), new DateTime(5555,5,5,5,5,5)};
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_Bool()
        {
            var formats = new[] {
                "{0:Yes|No}",
                "{1:Yes|No}",
            };
            var expected = new[] {
                "No",
                "Yes",
            };

            var args = new object[]{false, true};
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_Dates()
        {
            var formats = new[] {
                "{0:Past|Future} {1:Past|Future} {2:Past|Future}",
                "{0:Past|Present|Future} {1:Past|Present|Future} {2:Past|Present|Future}",
            };
            var expected = new[] {
                "Past Past Future",
                "Past Present Future",
            };

            // only the date part will be compared
            var args = new object[] {new DateTime(1111,1,1,1,1,1),SystemTime.Now(),new DateTime(5555,5,5,5,5,5)};
            Smart.Default.Test(formats, args, expected);
        }
        [Test]
        public void Test_DateTimeOffset_Dates()
        {
            var formats = new[] {
                "{0:Past|Future} {1:Past|Future} {2:Past|Future}",
                "{0:Past|Present|Future} {1:Past|Present|Future} {2:Past|Present|Future}",
            };
            var expected = new[] {
                "Past Past Future",
                "Past Present Future",
            };

            // only the date part will be compared
            var args = new object[]
                {SystemTime.OffsetNow().AddDays(-1), SystemTime.OffsetNow(), SystemTime.OffsetNow().AddDays(1)};
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_TimeSpan()
        {
            var formats = new[] {
                "{0:Past|Future} {1:Past|Future} {2:Past|Future}",
                "{0:Past|Zero|Future} {1:Past|Zero|Future} {2:Past|Zero|Future}",
            };
            var expected = new[] {
                "Past Past Future",
                "Past Zero Future",
            };

            var args = new object[] {new TimeSpan(-1,-1,-1,-1,-1), TimeSpan.Zero,new TimeSpan(5,5,5,5,5)};
            Smart.Default.Test(formats, args, expected);
        }

        [TestCase("{0:cond:{}|Empty}", "Hello")]
        [TestCase("{1:cond:{}|Empty}", "Empty")]
        [TestCase("{2:cond:{}|Null}", "Null")]
        public void Test_Strings(string format, string expected)
        {
            var args = new object[] { "Hello", "", null! };
            Smart.Default.Test(format, args, expected);
        }

        [TestCase("{0:cond:{}|Null}", "{ NotNull = True }")] // 'expected' comes from the default formatter, writing the anonymous type with 'ToString()'
        [TestCase("{1:cond:{}|Null}", "Null")]
        public void Test_Object(string format, string expected)
        {
            var args = new object[] {new {NotNull = true}, null!};
            Smart.Default.Test(format, args, expected);
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
