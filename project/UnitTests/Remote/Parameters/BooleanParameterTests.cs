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
    public class BooleanParameterTests
    {
        [Test]
        public void ConstructorSetsName()
        {
            var name = "newParam";
            var parameter = new BooleanParameter(name);
            ClassicAssert.AreEqual(name, parameter.Name);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void SetGetProperties()
        {
            var parameter = new BooleanParameter();
            var trueValue = new NameValuePair("TrueName", "TrueValue");
            var falseValue = new NameValuePair("FalseName", "FalseValue");
            parameter.TrueValue = trueValue;
            parameter.FalseValue = falseValue;
            ClassicAssert.AreEqual(typeof(string), parameter.DataType, "DataType does not match");
            ClassicAssert.AreSame(trueValue, parameter.TrueValue);
            ClassicAssert.AreSame(falseValue, parameter.FalseValue);
            ClassicAssert.IsNotNull(parameter.AllowedValues);
            ClassicAssert.AreEqual(2, parameter.AllowedValues.Length);
            ClassicAssert.AreEqual(trueValue.Name, parameter.AllowedValues[0]);
            ClassicAssert.AreEqual(falseValue.Name, parameter.AllowedValues[1]);

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
        }

        [Test]
        public void IsRequiredWithBlank()
        {
            var parameter = new BooleanParameter();
            parameter.Name = "Test";
            parameter.IsRequired = true;
            var results = parameter.Validate(string.Empty);
            ClassicAssert.AreEqual(1, results.Length, "Number of exceptions does not match");
            ClassicAssert.AreEqual("Value of 'Test' is required", results[0].Message, "Exception message does not match");
        }

        [Test]
        public void ConvertHandlesTrueValue()
        {
            var parameter = GenerateParameter();
            var value = parameter.Convert("trueName");
            ClassicAssert.AreEqual("trueValue", value);
        }

        [Test]
        public void ConvertHandlesFalseValue()
        {
            var parameter = GenerateParameter();
            var value = parameter.Convert("falseName");
            ClassicAssert.AreEqual("falseValue", value);
        }

        [Test]
        public void ConvertHandlesUnknown()
        {
            var parameter = GenerateParameter();
            var value = parameter.Convert("Unknown");
            ClassicAssert.AreEqual("Unknown", value);
        }

        private static BooleanParameter GenerateParameter()
        {
            var parameter = new BooleanParameter();
            parameter.TrueValue = new NameValuePair("trueName", "trueValue");
            parameter.FalseValue = new NameValuePair("falseName", "falseValue");
            return parameter;
        }
    }
}
