namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Parameters
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ThoughtWorks.CruiseControl.Remote.Parameters;
    using ThoughtWorks.CruiseControl.Remote;
    using NUnit.Framework.Legacy;

    [TestFixture]
    public class DateParameterTests
    {
        [Test]
        public void ConstructorSetsName()
        {
            var name = "newParam";
            var parameter = new DateParameter(name);
            ClassicAssert.AreEqual(name, parameter.Name);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void SetGetProperties()
        {
            var parameter = new DateParameter();
            var trueValue = new NameValuePair("TrueName", "TrueValue");
            var falseValue = new NameValuePair("FalseName", "FalseValue");
            ClassicAssert.IsNull(parameter.AllowedValues);
            ClassicAssert.AreEqual(typeof(DateTime), parameter.DataType, "DataType does not match");

            parameter.IsRequired = false;
            ClassicAssert.AreEqual(false, parameter.IsRequired, "IsRequired does not match");
            parameter.IsRequired = true;
            ClassicAssert.AreEqual(true, parameter.IsRequired, "IsRequired does not match");
            parameter.Description = "Some description goes here";
            ClassicAssert.AreEqual("Some description goes here", parameter.Description, "Description does not match");
            parameter.Name = "Some name";
            ClassicAssert.AreEqual("Some name", parameter.Name, "Name does not match");
            ClassicAssert.AreEqual("Some name", parameter.DisplayName, "DisplayName does not match");
            parameter.DisplayName = "Another name";
            ClassicAssert.AreEqual("Another name", parameter.DisplayName, "DisplayName does not match");

            var minValue = new DateTime(2010, 1, 1);
            parameter.MinimumValue = minValue;
            ClassicAssert.AreEqual(minValue, parameter.MinimumValue);

            var maxValue = new DateTime(2010, 1, 1);
            parameter.MaximumValue = maxValue;
            ClassicAssert.AreEqual(maxValue, parameter.MaximumValue);

            var defaultValue = "today";
            parameter.ClientDefaultValue = defaultValue;
            ClassicAssert.AreEqual(defaultValue, parameter.ClientDefaultValue);
        }

        [Test]
        public void IsRequiredWithBlank()
        {
            var parameter = new DateParameter();
            parameter.Name = "Test";
            parameter.IsRequired = true;
            var results = parameter.Validate(string.Empty);
            ClassicAssert.AreEqual(1, results.Length, "Number of exceptions does not match");
            ClassicAssert.AreEqual("Value of 'Test' is required", results[0].Message, "Exception message does not match");
        }

        [Test]
        public void CanGenerateDefault()
        {
            var parameter = new DateParameter();
            parameter.ClientDefaultValue = "today";
            parameter.GenerateClientDefault();
            ClassicAssert.AreEqual(DateTime.Today.ToShortDateString(), parameter.ClientDefaultValue);
        }

        [Test]
        public void ConvertHandlesEmptyString()
        {
            var parameter = new DateParameter();
            var actualValue = parameter.Convert(string.Empty);
            ClassicAssert.AreEqual(DateTime.Today, actualValue);
        }

        [Test]
        public void ConvertHandlesToday()
        {
            var parameter = new DateParameter();
            var actualValue = parameter.Convert("Today");
            ClassicAssert.AreEqual(DateTime.Today, actualValue);
        }

        [Test]
        public void ConvertHandlesAddition()
        {
            var parameter = new DateParameter();
            var actualValue = parameter.Convert("Today+2");
            ClassicAssert.AreEqual(DateTime.Today.AddDays(2), actualValue);
        }

        [Test]
        public void ConvertHandlesSubtraction()
        {
            var parameter = new DateParameter();
            var actualValue = parameter.Convert("Today-2");
            ClassicAssert.AreEqual(DateTime.Today.AddDays(-2), actualValue);
        }

        [Test]
        public void ConvertFailsWhenOperationIsUnknown()
        {
            var parameter = new DateParameter();
            var error = ClassicAssert.Throws<InvalidOperationException>(() =>
            {
                parameter.Convert("Today*2");
            });
            ClassicAssert.AreEqual("Unknown operation: '*'", error.Message);
        }

        [Test]
        public void ConvertHandlesDayOfWeek()
        {
            var parameter = new DateParameter();
            var actualValue = parameter.Convert("dayofweek(3)");
            ClassicAssert.AreEqual(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 3), actualValue);
        }

        [Test]
        public void ConvertHandlesDayOfMonth()
        {
            var parameter = new DateParameter();
            var actualValue = parameter.Convert("dayofmonth(10)");
            ClassicAssert.AreEqual(DateTime.Today.AddDays(-DateTime.Today.Day+10), actualValue);
        }

        [Test]
        public void ConvertHandlesDate()
        {
            var parameter = new DateParameter();
            var actualValue = parameter.Convert("2010-01-01");
            ClassicAssert.AreEqual(new DateTime(2010, 1, 1), actualValue);
        }

        [Test]
        public void ValidateChecksThatTheValueIsADate()
        {
            var parameter = new DateParameter("Test");
            var exceptions = parameter.Validate("notadate!");
            ClassicAssert.AreEqual(1, exceptions.Length);
            ClassicAssert.AreEqual("Value of 'Test' is not a date", exceptions[0].Message);
        }

        [Test]
        public void ValidateChecksTheDateIsLessThanMaximum()
        {
            var parameter = new DateParameter("Test");
            parameter.MaximumValue = new DateTime(2010, 1, 1);
            var exceptions = parameter.Validate("2010-1-31");
            ClassicAssert.AreEqual(1, exceptions.Length);
            ClassicAssert.AreEqual("Value of 'Test' is more than the maximum allowed (01/01/2010)", exceptions[0].Message);
        }

        [Test]
        public void ValidateChecksTheDateIsMoreThanMinimum()
        {
            var parameter = new DateParameter("Test");
            parameter.MinimumValue = new DateTime(2010, 1, 31);
            var exceptions = parameter.Validate("2010-1-1");
            ClassicAssert.AreEqual(1, exceptions.Length);
            ClassicAssert.AreEqual("Value of 'Test' is less than the minimum allowed (01/31/2010)", exceptions[0].Message);
        }
    }
}
