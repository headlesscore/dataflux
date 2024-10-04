using Exortech.NetReflector;
using Xunit;

using System;
using System.Collections.Generic;
using System.Reflection;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    
    public class DynamicValueUtilityTests
    {
        [Fact]
        public void SplitPropertyIntoPartsSingleValue()
        {
            DynamicValueUtility.PropertyPart[] parts = DynamicValueUtility.SplitPropertyName("Test");
            Assert.True(1 == parts.Length, "Length differs from expected");
            Assert.True(true);
            Assert.True(true);
            CheckPart(parts[0], 0, "Test", null, null);
        }

        [Fact]
        public void SplitPropertyIntoPartsSingleValueWithKey()
        {
            DynamicValueUtility.PropertyPart[] parts = DynamicValueUtility.SplitPropertyName("Test[Key=Value]");
            Assert.Single(parts, "Length differs from expected");
            
            CheckPart(parts[0], 0, "Test", "Key", "Value");
        }

        [Fact]
        public void SplitPropertyIntoPartsMultipleValues()
        {
            DynamicValueUtility.PropertyPart[] parts = DynamicValueUtility.SplitPropertyName("Test.Part2.Part3");
            Assert.True(3 == parts.Length, "Length differs from expected");
            CheckPart(parts[0], 0, "Test", null, null);
            CheckPart(parts[1], 1, "Part2", null, null);
            CheckPart(parts[2], 2, "Part3", null, null);
        }

        [Fact]
        public void SplitPropertyIntoPartsMultipleValuesWithKey()
        {
            DynamicValueUtility.PropertyPart[] parts = DynamicValueUtility.SplitPropertyName("Test.Part2[Key=Value].Part3");
            Assert.True(3 == parts.Length, "Length differs from expected");
            CheckPart(parts[0], 0, "Test", null, null);
            CheckPart(parts[1], 1, "Part2", "Key", "Value");
            CheckPart(parts[2], 2, "Part3", null, null);
        }

        private void CheckPart(DynamicValueUtility.PropertyPart part, int position, string name, string keyName, string keyValue)
        {
            Assert.True(name == part.Name, string.Format(System.Globalization.CultureInfo.CurrentCulture, $"Part name does not match [{position}]"));
            Assert.True(keyName == part.KeyName, string.Format(System.Globalization.CultureInfo.CurrentCulture, $"Part key name does not match [{position}]"));
            Assert.True(keyValue == part.KeyValue, string.Format(System.Globalization.CultureInfo.CurrentCulture, $"Part key value does not match [{position}]"));
        }

        [Fact]
        public void FindActualPropertyWithValidProperty()
        {
            TestClass testValue = new TestClass();
            testValue.Name = "My name";
            MemberInfo property = DynamicValueUtility.FindActualProperty(testValue, "someName");
            Assert.NotNull(property);
            Assert.True("Name" == property.Name, "Property names do not match");
        }

        [Fact]
        public void FindActualPropertyWithInvalidProperty()
        {
            TestClass testValue = new TestClass();
            testValue.Name = "My name";
            object property = DynamicValueUtility.FindActualProperty(testValue, "Name");
            Assert.Null(property);
        }

        [Fact]
        public void FindKeyedValueWithActualValue()
        {
            TestClass testValue = new TestClass();
            TestClass subValue = new TestClass();
            subValue.Name = "A value";
            testValue.SubValues = new TestClass[]{
                subValue
            };
            object result = DynamicValueUtility.FindKeyedValue(testValue.SubValues, "someName", "A value");
            Assert.NotNull(result);
            Assert.True(subValue == result, "Found value does not match");
        }

        [Fact]
        public void FindTypedValueWithActualValue()
        {
            TestClass testValue = new TestClass();
            TestClass subValue = new TestClass();
            testValue.SubValues = new TestClass[]{
                subValue
            };
            object result = DynamicValueUtility.FindTypedValue(testValue.SubValues, "testInstance");
            Assert.NotNull(result);
            Assert.True(subValue == result, "Found value does not match");
        }

        [Fact]
        public void FindPropertySingle()
        {
            TestClass rootValue = new TestClass("root");
            DynamicValueUtility.PropertyValue result = DynamicValueUtility.FindProperty(rootValue, "someName");
            Assert.NotNull(result);
            Assert.True("Name" == result.Property.Name, "Property names do not match");
            Assert.True("root" == result.Value as string, "Property values do not match");
        }

        [Fact]
        public void FindPropertyMultiple()
        {
            TestClass rootValue = new TestClass("root");
            rootValue.SubValues = new TestClass[] {
                new TestClass("child1"),
                new TestClass("child2")
            };
            DynamicValueUtility.PropertyValue result = DynamicValueUtility.FindProperty(rootValue, "sub.testInstance.someName");
            Assert.NotNull(result, "Property not found");
            Assert.True("Name" == result.Property.Name, "Property names do not match");
            Assert.True("child2" == result.Value as string, "Property values do not match");
        }

        [Fact]
        public void FindPropertyMultipleWithKey()
        {
            TestClass rootValue = new TestClass("root");
            rootValue.SubValues = new TestClass[] {
                new TestClass("child1"),
                new TestClass("child2")
            };
            DynamicValueUtility.PropertyValue result = DynamicValueUtility.FindProperty(rootValue, "sub[someName=child1].someName");
            Assert.NotNull(result);
            Assert.True("Name" == result.Property.Name, "Property names do not match");
            Assert.True("child1" == result.Value as string, "Property values do not match");
        }

        [Fact]
        public void FindPropertyMultipleWithIndex()
        {
            TestClass rootValue = new TestClass("root");
            rootValue.SubValues = new TestClass[] {
                new TestClass("child1"),
                new TestClass("child2")
            };
            DynamicValueUtility.PropertyValue result = DynamicValueUtility.FindProperty(rootValue, "sub[0].someName");
            Assert.NotNull(result);
            Assert.True("Name" == result.Property.Name, "Property names do not match");
            Assert.True("child1" == result.Value as string, "Property values do not match");
        }

        [Fact]
        public void ChangePropertySameType()
        {
            TestClass rootValue = new TestClass("root");
            DynamicValueUtility.PropertyValue result = DynamicValueUtility.FindProperty(rootValue, "someName");
            result.ChangeProperty("nonRoot");
            Assert.Equal("nonRoot", rootValue.Name, "Property not changed");
        }

        [Fact]
        public void ChangePropertyDifferentType()
        {
            TestClass rootValue = new TestClass("root");
            rootValue.Value = 100;
            DynamicValueUtility.PropertyValue result = DynamicValueUtility.FindProperty(rootValue, "aValue");
            result.ChangeProperty("20");
            Assert.Equal(20, rootValue.Value, "Property not changed");
        }

        [Fact]
        public void DynamicUtilityNestedTasksWithParameters_EmptyReflectorTable()
        {
            var TaskSetupXml = GetNestedTasksWithParametersXML();
            var processedTaskXml = "<conditional>" +
                "  <conditions>" +
                "    <buildCondition>" +
                "      <value>ForceBuild</value>" +
                "    </buildCondition>" +
                "    <compareCondition>" +
                "      <value1></value1>" +
                "      <value2>Yes</value2>" +
                "      <evaluation>Equal</evaluation>" +
                "    </compareCondition>" +
                "  </conditions>" +
                "  <tasks>" +
                "    <exec>" +
                "      <!-- if you want the task to fail, ping an unknown server -->" +
                "      <executable>ping.exe</executable>" +
                "      <buildArgs>localhost</buildArgs>" +
                "      <buildTimeoutSeconds>15</buildTimeoutSeconds>" +
                "      <description>Pinging a server</description>" +
                "    </exec>" +
                "    <conditional>" +
                "      <conditions>" +
                "        <compareCondition>" +
                "          <value1></value1>" +
                "          <value2>Yes</value2>" +
                "          <evaluation>Equal</evaluation>" +
                "        </compareCondition>" +
                "      </conditions>" +
                "      <tasks>" +
                "        <exec>" +
                "          <!-- if you want the task to fail, ping an unknown server -->" +
                "          <executable>ping.exe</executable>" +
                "          <buildArgs></buildArgs>" +
                "          <buildTimeoutSeconds>15</buildTimeoutSeconds>" +
                "          <description>Pinging a server</description>" +
                "        </exec>" +
                "      </tasks>" +
                "    </conditional>" +
                "  </tasks>" +
                "  <dynamicValues>" +
                "    <directValue>" +
                "      <parameter>CommitBuild</parameter>" +
                "      <property>conditions.compareCondition.value1</property>" +
                "    </directValue>" +
                "    <directValue>" +
                "      <parameter>TagBuild</parameter>" +
                "      <property>tasks.conditional.conditions.compareCondition.value1</property>" +
                "    </directValue>" +
                "    <directValue>" +
                "      <parameter>TagVersion</parameter>" +
                "      <property>tasks.conditional.tasks.exec.buildArgs</property>" +
                "    </directValue>" +
                "  </dynamicValues>" +
                "</conditional>";

            var xdoc = new System.Xml.XmlDocument();
            xdoc.LoadXml(TaskSetupXml);
            Exortech.NetReflector.NetReflectorTypeTable typeTable = new Exortech.NetReflector.NetReflectorTypeTable();

            var result = ThoughtWorks.CruiseControl.Core.Tasks.DynamicValueUtility.ConvertXmlToDynamicValues(typeTable, xdoc.DocumentElement, null);
            Console.WriteLine(result.OuterXml);

            xdoc.LoadXml(processedTaskXml); // load in xdoc to ease comparing xml documents

            Assert.Equal(xdoc.OuterXml, result.OuterXml);
        }

        [Fact]
        public void DynamicUtilityNestedTasksWithParameters_ReflectorTableInitialisedAsByServer()
        {
            var TaskSetupXml = GetNestedTasksWithParametersXML();
            var processedTaskXml = "<conditional>" +
                    "  <conditions>" +
                    "    <buildCondition>" +
                    "      <value>ForceBuild</value>" +
                    "    </buildCondition>" +
                    "    <compareCondition>" +
                    "      <value1></value1>" +
                    "      <value2>Yes</value2>" +
                    "      <evaluation>Equal</evaluation>" +
                    "    </compareCondition>" +
                    "  </conditions>" +
                    "  <tasks>" +
                    "    <exec>" +
                    "      <!-- if you want the task to fail, ping an unknown server -->" +
                    "      <executable>ping.exe</executable>" +
                    "      <buildArgs>localhost</buildArgs>" +
                    "      <buildTimeoutSeconds>15</buildTimeoutSeconds>" +
                    "      <description>Pinging a server</description>" +
                    "    </exec>" +
                    "    <conditional>" +
                    "      <conditions>" +
                    "        <compareCondition>" +
                    "          <value1></value1>" +
                    "          <value2>Yes</value2>" +
                    "          <evaluation>Equal</evaluation>" +
                    "        </compareCondition>" +
                    "      </conditions>" +
                    "      <tasks>" +
                    "        <exec>" +
                    "          <!-- if you want the task to fail, ping an unknown server -->" +
                    "          <executable>ping.exe</executable>" +
                    "          <buildArgs></buildArgs>" +
                    "          <buildTimeoutSeconds>15</buildTimeoutSeconds>" +
                    "          <description>Pinging a server</description>" +
                    "          <dynamicValues>" +
                    "            <directValue>" +
                    "              <parameter>TagVersion</parameter>" +
                    "              <property>buildArgs</property>" +
                    "            </directValue>" +
                    "          </dynamicValues>" +
                    "        </exec>" +
                    "      </tasks>" +
                    "      <dynamicValues>" +
                    "        <directValue>" +
                    "          <parameter>TagBuild</parameter>" +
                    "          <property>conditions[0].value1</property>" +
                    "        </directValue>" +
                    "      </dynamicValues>" +
                    "    </conditional>" +
                    "  </tasks>" +
                    "  <dynamicValues>" +
                    "    <directValue>" +
                    "      <parameter>CommitBuild</parameter>" +
                    "      <property>conditions[1].value1</property>" +
                    "    </directValue>" +
                    "  </dynamicValues>" +
                    "</conditional>";


            var xdoc = new System.Xml.XmlDocument();
            xdoc.LoadXml(TaskSetupXml);

            Objection.ObjectionStore objectionStore = new Objection.ObjectionStore();
            Exortech.NetReflector.NetReflectorTypeTable typeTable = Exortech.NetReflector.NetReflectorTypeTable.CreateDefault(new Objection.NetReflectorPlugin.ObjectionNetReflectorInstantiator(objectionStore));

            var result = ThoughtWorks.CruiseControl.Core.Tasks.DynamicValueUtility.ConvertXmlToDynamicValues(typeTable, xdoc.DocumentElement, null);
            Console.WriteLine(result.OuterXml);

            xdoc.LoadXml(processedTaskXml); // load in xdoc to ease comparing xml documents

            Assert.Equal(xdoc.OuterXml, result.OuterXml);
        }

        private string GetNestedTasksWithParametersXML()
        {
            return "	<conditional>" +
                "			<conditions>" +
                "				<buildCondition>" +
                "					<value>ForceBuild</value>" +
                "				</buildCondition>" +
                "				<compareCondition>" +
                "					<value1>$[CommitBuild]</value1>" +
                "					<value2>Yes</value2>" +
                "					<evaluation>Equal</evaluation>" +
                "				</compareCondition>" +
                "			</conditions>" +
                "			<tasks>" +
                "				<exec>" +
                "					<!-- if you want the task to fail, ping an unknown server -->" +
                "					<executable>ping.exe</executable>" +
                "					<buildArgs>localhost</buildArgs>" +
                "					<buildTimeoutSeconds>15</buildTimeoutSeconds>" +
                "					<description>Pinging a server</description>" +
                "				</exec>" +
                "				<conditional>" +
                "					<conditions>" +
                "						<compareCondition>" +
                "							<value1>$[TagBuild]</value1>" +
                "							<value2>Yes</value2>" +
                "							<evaluation>Equal</evaluation>" +
                "						</compareCondition>" +
                "					</conditions>" +
                "					<tasks>" +
                "						<exec>" +
                "							<!-- if you want the task to fail, ping an unknown server -->" +
                "							<executable>ping.exe</executable>" +
                "							<buildArgs>$[TagVersion]</buildArgs>" +
                "							<buildTimeoutSeconds>15</buildTimeoutSeconds>" +
                "							<description>Pinging a server</description>" +
                "						</exec>					" +
                "					</tasks>" +
                "				</conditional>" +
                "				" +
                "			</tasks>" +
                "		</conditional>";
        }


        [ReflectorType("testInstance")]
        public class TestClass
        {
            private string myName;
            private int myValue;
            private TestClass[] mySubValues;

            public TestClass() { }
            public TestClass(string name) { this.myName = name; }

            [ReflectorProperty("someName")]
            public string Name
            {
                get { return myName; }
                set { myName = value; }
            }

            [ReflectorProperty("aValue")]
            public int Value
            {
                get { return myValue; }
                set { myValue = value; }
            }

            [ReflectorProperty("sub")]
            public TestClass[] SubValues
            {
                get { return mySubValues; }
                set { mySubValues = value; }
            }
        }
    }
}
