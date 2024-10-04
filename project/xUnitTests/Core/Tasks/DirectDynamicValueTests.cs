﻿using Exortech.NetReflector;
using Xunit;

using System;
using System.Collections.Generic;
using System.Reflection;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    
    public class DirectDynamicValueTests
    {
        [Fact]
        public void SetGetProperties()
        {
            DirectDynamicValue value = new DirectDynamicValue();
            value.ParameterName = "test parameter";
            Assert.Equal("test parameter", value.ParameterName, "ParameterName not being get/set correctly");
            value.PropertyName = "test property";
            Assert.Equal("test property", value.PropertyName, "PropertyName not being get/set correctly");
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ApplyTo()
        {
            MsBuildTask testTask = new MsBuildTask();
            DirectDynamicValue value = new DirectDynamicValue("newDir", "workingDirectory");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("newDir", "a location");
            value.ApplyTo(testTask, parameters, null);
            Assert.Equal("a location", testTask.WorkingDirectory, "Value has not been correctly set");
        }
    }
}
