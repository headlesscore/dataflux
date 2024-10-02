using System;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Remote.Messages;
using ThoughtWorks.CruiseControl.Remote;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    [TestFixture]
    public class ChangeConfigurationRequestTests
    {
        [Test]
        public void GetSetAllPropertiesWorks()
        {
            ChangeConfigurationRequest request = new ChangeConfigurationRequest();
            request.ProjectDefinition = "Build#1";
            ClassicAssert.AreEqual("Build#1", request.ProjectDefinition, "ProjectDefinition fails the get/set test");
            request.PurgeArtifactDirectory = true;
            ClassicAssert.IsTrue(request.PurgeArtifactDirectory, "PurgeArtifactDirectory fails the get/set test");
            request.PurgeSourceControlEnvironment = true;
            ClassicAssert.IsTrue(request.PurgeSourceControlEnvironment, "PurgeSourceControlEnvironment fails the get/set test");
            request.PurgeWorkingDirectory = true;
            ClassicAssert.IsTrue(request.PurgeWorkingDirectory, "PurgeWorkingDirectory fails the get/set test");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void InitialiseRequestWithSessionSetsTheCorrectValues()
        {
            string sessionToken = "the session";
            DateTime now = DateTime.Now;
            ChangeConfigurationRequest request = new ChangeConfigurationRequest(sessionToken);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(request.Identifier), "Identifier was not set");
            ClassicAssert.AreEqual(Environment.MachineName, request.SourceName, "Source name doesn't match the machine name");
            ClassicAssert.AreEqual(sessionToken, request.SessionToken, "SessionToken doesn't match the input token");
            ClassicAssert.IsTrue((now <= request.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void InitialiseRequestWithSessionAndProjectSetsTheCorrectValues()
        {
            string sessionToken = "the session";
            string projectName = "the project";
            DateTime now = DateTime.Now;
            ChangeConfigurationRequest request = new ChangeConfigurationRequest(sessionToken, projectName);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(request.Identifier), "Identifier was not set");
            ClassicAssert.AreEqual(Environment.MachineName, request.SourceName, "Source name doesn't match the machine name");
            ClassicAssert.AreEqual(sessionToken, request.SessionToken, "SessionToken doesn't match the input token");
            ClassicAssert.AreEqual(projectName, request.ProjectName, "ProjectName doesn't match the input project name");
            ClassicAssert.IsTrue((now <= request.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void ToStringSerialisesDefaultValues()
        {
            ChangeConfigurationRequest request = new ChangeConfigurationRequest();
            string actual = request.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<changeConfigurationRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{2:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" source=\"{1}\" " +
                "purgeWorking=\"false\" purgeArtifact=\"false\" purgeSourceControl=\"false\" />",
                request.Identifier,
                request.SourceName,
                request.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }

        [Test]
        public void ToStringSerialisesAllValues()
        {
            ChangeConfigurationRequest request = new ChangeConfigurationRequest();
            request.Identifier = "identifier";
            request.ServerName = "serverName";
            request.SessionToken = "sessionToken";
            request.SourceName = "sourceName";
            request.Timestamp = DateTime.Now;
            request.ProjectDefinition = "Build#1";
            request.PurgeArtifactDirectory = true;
            request.PurgeSourceControlEnvironment = true;
            request.PurgeWorkingDirectory = true;
            string actual = request.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<changeConfigurationRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{4:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" server=\"{1}\" source=\"{2}\" session=\"{3}\" " +
                "purgeWorking=\"true\" purgeArtifact=\"true\" purgeSourceControl=\"true\">" + 
                "<definition>Build#1</definition>" + 
                "</changeConfigurationRequest>",
                request.Identifier,
                request.ServerName,
                request.SourceName,
                request.SessionToken,
                request.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }
    }
}
