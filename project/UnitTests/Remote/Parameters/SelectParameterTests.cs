namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Parameters
{
    using System;
    using System.IO;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.Remote.Parameters;

    [TestFixture]
    public class SelectParameterTests
    {
        [Test]
        public void SetGetProperties()
        {
            SelectParameter parameter = new SelectParameter();
            ClassicAssert.AreEqual(typeof(string), parameter.DataType, "DataType does not match");

            parameter.IsRequired = false;
            ClassicAssert.AreEqual(false, parameter.IsRequired, "IsRequired does not match");
            parameter.IsRequired = true;
            ClassicAssert.AreEqual(true, parameter.IsRequired, "IsRequired does not match");
            parameter.DataValues = new NameValuePair[] { 
                new NameValuePair(string.Empty, "Dev"), 
                new NameValuePair("Test", "Test"), 
                new NameValuePair(null, "Prod") 
            };
            ClassicAssert.AreEqual(3, parameter.AllowedValues.Length, "AllowedValues does not match");
            parameter.Description = "Some description goes here";
            ClassicAssert.AreEqual("Some description goes here", parameter.Description, "Description does not match");
            parameter.Name = "Some name";
            ClassicAssert.AreEqual("Some name", parameter.Name, "Name does not match");
            ClassicAssert.AreEqual("Some name", parameter.DisplayName, "DisplayName does not match");
            parameter.DisplayName = "Another name";
            ClassicAssert.AreEqual("Another name", parameter.DisplayName, "DisplayName does not match");

            var defaultValue = "daDefault";
            var clientValue = "daDefaultForDaClient";
            parameter.DefaultValue = defaultValue;
            ClassicAssert.AreEqual(defaultValue, parameter.DefaultValue);
            ClassicAssert.AreEqual(defaultValue, parameter.ClientDefaultValue);
            parameter.ClientDefaultValue = clientValue;
            ClassicAssert.AreEqual(clientValue, parameter.ClientDefaultValue);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void DefaultValueChecksAllowedValues()
        {
            var parameter = new SelectParameter();
            parameter.DataValues = new NameValuePair[] 
            {
                new NameValuePair("name1", "value1"),
                new NameValuePair("name2", "value2")
            };
            parameter.DefaultValue = "value2";
            ClassicAssert.AreEqual("name2", parameter.ClientDefaultValue);
        }

        [Test]
        public void ConvertReturnsValueForName()
        {
            var parameter = new SelectParameter();
            parameter.DataValues = new NameValuePair[] 
            {
                new NameValuePair("name1", "value1"),
                new NameValuePair("name2", "value2")
            };
            var value = parameter.Convert("name1");
            ClassicAssert.AreEqual("value1", value);
        }

        [Test]
        public void ConvertReturnsOriginalIfNameNotFound()
        {
            var parameter = new SelectParameter();
            parameter.DataValues = new NameValuePair[] 
            {
                new NameValuePair("name1", "value1"),
                new NameValuePair("name2", "value2")
            };
            var value = parameter.Convert("notFound");
            ClassicAssert.AreEqual("notFound", value);
        }

        [Test]
        public void IsRequiredWithBlank()
        {
            SelectParameter parameter = new SelectParameter();
            parameter.Name = "Test";
            parameter.IsRequired = true;
            Exception[] results = parameter.Validate(string.Empty);
            ClassicAssert.AreEqual(1, results.Length, "Number of exceptions does not match");
            ClassicAssert.AreEqual("Value of 'Test' is required", results[0].Message, "Exception message does not match");
        }

        [Test]
        public void IsAllowedValue()
        {
            SelectParameter parameter = new SelectParameter();
            parameter.Name = "Test";
            parameter.DataValues = new NameValuePair[] { 
                new NameValuePair(string.Empty, "Dev"), 
                new NameValuePair("Test", "Test"), 
                new NameValuePair(null, "Prod") 
            };
            Exception[] results = parameter.Validate("Dev");
            ClassicAssert.AreEqual(0, results.Length, "Number of exceptions does not match");
        }

        [Test]
        public void IsNotAllowedValue()
        {
            SelectParameter parameter = new SelectParameter();
            parameter.Name = "Test";
            parameter.DataValues = new NameValuePair[] { 
                new NameValuePair(string.Empty, "Dev"), 
                new NameValuePair("Test", "Test"), 
                new NameValuePair(null, "Prod") 
            };
            Exception[] results = parameter.Validate("QA");
            ClassicAssert.AreEqual(1, results.Length, "Number of exceptions does not match");
            ClassicAssert.AreEqual("Value of 'Test' is not an allowed value", results[0].Message, "Exception message does not match");
        }

        [Test]
        public void CanGetSetDataValues()
        {
            var parameter = new SelectParameter();
            var dataValues = new NameValuePair[] 
            {
                new NameValuePair("one", "1"),
                new NameValuePair("two", "2"),
                new NameValuePair("three", "3")
            };
            parameter.DataValues = dataValues;
            ClassicAssert.AreSame(dataValues, parameter.DataValues);
        }

        [Test]
        public void GenerateClientDefaultLoadsFromAFile()
        {
            var sourceFile = Path.GetTempFileName();
            try
            {
                var lines = new string[] 
                {
                    "Option #1",
                    "Option #2"
                };
                File.WriteAllLines(sourceFile, lines);
                var parameter = new SelectParameter();
                parameter.SourceFile = sourceFile;
                parameter.GenerateClientDefault();
                CollectionAssert.AreEqual(lines, parameter.AllowedValues);
            }
            finally
            {
                File.Delete(sourceFile);
            }
        }
    }
}
