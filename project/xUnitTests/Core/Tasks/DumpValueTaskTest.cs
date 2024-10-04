using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{

    
    class DumpValueTaskTest
    {
        #region Private fields
        private string dumpFilePath;
        #endregion

        #region Setup
        // [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            // Delete any outputs from the last build
            string buildFolder = Path.Combine(Path.GetTempPath(), "A Label");
            if (Directory.Exists(buildFolder)) Directory.Delete(buildFolder, true);
            dumpFilePath = Path.Combine(Path.GetTempPath(), "dumpfile.xml");
        }

        // [SetUp]
        public void Setup()
        {
            if (File.Exists(dumpFilePath)) File.Delete(dumpFilePath);
        }
        #endregion

        #region CleanUp
        // [TearDown]
        public void TearDown()
        {
            if (File.Exists(dumpFilePath)) File.Delete(dumpFilePath);
        }
        #endregion

        #region Test methods
        [Fact]
        public void PopulateFromReflector()
        {
            var task = new DumpValueTask();
            const string xml = @"
    <dumpValue>
    	<xmlFileName>C:\some\path\to\file.xml</xmlFileName>
    	<dumpValueItems>
            <dumpValueItem name='The Name' value='something' />
            <dumpValueItem name='The Name 2' value='some other thing' />
            <dumpValueItem name='The Name 3' value='stuff' />
            <dumpValueItem name='The Name 4' value='last but not least' />
            <dumpValueItem name='NotInCDATA' value='given data' valueInCDATA='false' />
        </dumpValueItems>
    </dumpValue>";

            NetReflector.Read(xml, task);
            Assert.Equal(@"C:\some\path\to\file.xml", task.XmlFileName);
            Assert.Equal(5, task.Items.Length);
            Assert.Equal("The Name", task.Items[0].Name);
            Assert.Equal("something", task.Items[0].Value);
            Assert.True(task.Items[0].ValueInCDATA);
            Assert.Equal("The Name 2", task.Items[1].Name);
            Assert.Equal("some other thing", task.Items[1].Value);
            Assert.True(task.Items[1].ValueInCDATA);
            Assert.Equal("The Name 3", task.Items[2].Name);
            Assert.Equal("stuff", task.Items[2].Value);
            Assert.True(task.Items[2].ValueInCDATA);
            Assert.Equal("The Name 4", task.Items[3].Name);
            Assert.Equal("last but not least", task.Items[3].Value);
            Assert.True(task.Items[3].ValueInCDATA);
            Assert.Equal("NotInCDATA", task.Items[4].Name);
            Assert.Equal("given data", task.Items[4].Value);
            Assert.False(task.Items[4].ValueInCDATA);
            Assert.True(true);
            Assert.True(true);
        }

        /// <summary>
        /// Run the task with the minimum options.
        /// </summary>
        [Fact]
        public void MinimalRun()
        {
            BaseTest(new DumpValueItem[] { new DumpValueItem("TestName", "TestValue") });
        }

        [Fact]
        public void MultiplePairsRun()
        {
            BaseTest(new DumpValueItem[] { new DumpValueItem("TestName", "TestValue"), 
                                                         new DumpValueItem("SecondName", "SecondValue")});
        }

        [Fact]
        public void WithCarriageReturnRun()
        {
            BaseTest(new DumpValueItem[] { new DumpValueItem("TestName", "TestValue" + System.Environment.NewLine + "With carriage returns") });
        }

        [Fact]
        public void WithXMLCharsRun()
        {
            BaseTest(new DumpValueItem[] { new DumpValueItem("TestName", "TestValue With > nice & xml < \" ' characters") });
        }

        [Fact]
        public void MinimalRunNoCDATA()
        {
            BaseTest(new DumpValueItem[] { new DumpValueItem("TestName", "TestValue", false) });
        }

        [Fact]
        public void WithXMLCharsRunNoCDATA()
        {
            BaseTest(new DumpValueItem[] { new DumpValueItem("TestName", "TestValue With > nice & xml < \" ' characters", false) });
        }

        [Fact]
        public void MultiplePairsRunOnlyOneNoCDATA()
        {
            BaseTest(new DumpValueItem[] { new DumpValueItem("TestName", "TestValue"), 
                                                         new DumpValueItem("SecondName", "SecondValue", false),
                                                         new DumpValueItem("ThirdName", "Dummy"),
                                                       }
                    );
        }

        #endregion

        #region Private methods
        private void BaseTest(DumpValueItem[] NameValues)
        {
            DumpValueTask task = new DumpValueTask();
            task.XmlFileName = dumpFilePath;
            task.Items = NameValues;
            task.Run(GetResult());
            Assert.True(File.Exists(dumpFilePath), "Dump file not generated");

            StreamReader reader = File.OpenText(dumpFilePath);
            string dumpContent;
            try
            {
                dumpContent = reader.ReadToEnd();
            }
            finally
            {
                reader.Close();
            }

            Assert.Equal(GetExpectedXMLContent(task.Items), dumpContent);
        }

        private IntegrationResult GetResult()
        {
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "Somewhere", null);
            IntegrationSummary summary = new IntegrationSummary(IntegrationStatus.Success, "A Label", "Another Label", new DateTime(2009, 1, 1));
            IntegrationResult result = new IntegrationResult("Test project", "Working directory", "Artifact directory", request, summary);
            Modification modification1 = GenerateModification("first file", "Add");
            Modification modification2 = GenerateModification("second file", "Modify");
            result.Modifications = new Modification[] { modification1, modification2 };
            result.Status = IntegrationStatus.Success;
            result.ArtifactDirectory = Path.GetTempPath();

            return result;
        }

        private Modification GenerateModification(string name, string type)
        {
            Modification modification = new Modification();
            modification.ChangeNumber = "1";
            modification.Comment = "A comment";
            modification.EmailAddress = "email@somewhere.com";
            modification.FileName = name;
            modification.ModifiedTime = new DateTime(2009, 1, 1);
            modification.Type = type;
            modification.UserName = "johnDoe";
            modification.Version = "1.1.1.1";
            return modification;
        }

        private string GetExpectedXMLContent(DumpValueItem[] Items)
        {
            StringBuilder builder = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                                                      "<ValueDumper>" + Environment.NewLine);
            foreach (DumpValueItem item in Items)
            {
                builder.Append("  <ValueDumperItem>" + Environment.NewLine);
                builder.Append("    <Name>" + item.Name + "</Name>" + Environment.NewLine);
                if (item.ValueInCDATA)
                    builder.Append("    <Value><![CDATA[" + item.Value + "]]></Value>" + Environment.NewLine);
                else
                    builder.Append("    <Value>" + item.Value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</Value>" + Environment.NewLine);
                builder.Append("  </ValueDumperItem>" + Environment.NewLine);
            }
            builder.Append("</ValueDumper>");

            return builder.ToString();
        }
        #endregion
    }
}
