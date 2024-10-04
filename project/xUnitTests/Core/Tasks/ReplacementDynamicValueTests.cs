using Exortech.NetReflector;
using Xunit;

using System;
using System.Collections.Generic;
using System.Reflection;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    
    public class ReplacementDynamicValueTests
    {
        [Fact]
        public void SetGetProperties()
        {
            ReplacementDynamicValue value = new ReplacementDynamicValue();
            value.FormatValue = "test parameter";
            Assert.Equal("test parameter", value.FormatValue, "FormatValue not being get/set correctly");
            Assert.True(true);
            Assert.True(true);
            value.PropertyName = "test property";
            Assert.Equal("test property", value.PropertyName, "PropertyName not being get/set correctly");
            var parameters = new NameValuePair[] 
            {
                new NameValuePair("name", "value")
            };
            value.Parameters = parameters;
            Assert.Same(parameters, value.Parameters);
        }

        [Fact]
        public void ApplyTo()
        {
            MsBuildTask testTask = new MsBuildTask();
            ReplacementDynamicValue value = new ReplacementDynamicValue("{0}\\Happy - {1}", "workingDirectory", 
                new NameValuePair("newDir", null),
                new NameValuePair("oldDir", "default"));
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("newDir", "a location");
            value.ApplyTo(testTask, parameters, null);
            Assert.Equal("a location\\Happy - default", testTask.WorkingDirectory, "Value has not been correctly set");
        }
    }
}
