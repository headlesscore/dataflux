using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Reflection;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    [TestFixture]
    public class ReplacementDynamicValueTests
    {
        [Test]
        public void SetGetProperties()
        {
            ReplacementDynamicValue value = new ReplacementDynamicValue();
            value.FormatValue = "test parameter";
            ClassicAssert.AreEqual("test parameter", value.FormatValue, "FormatValue not being get/set correctly");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            value.PropertyName = "test property";
            ClassicAssert.AreEqual("test property", value.PropertyName, "PropertyName not being get/set correctly");
            var parameters = new NameValuePair[] 
            {
                new NameValuePair("name", "value")
            };
            value.Parameters = parameters;
            ClassicAssert.AreSame(parameters, value.Parameters);
        }

        [Test]
        public void ApplyTo()
        {
            MsBuildTask testTask = new MsBuildTask();
            ReplacementDynamicValue value = new ReplacementDynamicValue("{0}\\Happy - {1}", "workingDirectory", 
                new NameValuePair("newDir", null),
                new NameValuePair("oldDir", "default"));
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("newDir", "a location");
            value.ApplyTo(testTask, parameters, null);
            ClassicAssert.AreEqual("a location\\Happy - default", testTask.WorkingDirectory, "Value has not been correctly set");
        }
    }
}
