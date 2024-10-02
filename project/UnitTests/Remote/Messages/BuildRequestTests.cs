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
    public class BuildRequestTests
    {
        [Test]
        public void GetSetAllPropertiesWorks()
        {
            BuildRequest request = new BuildRequest();
            request.BuildName = "Build#1";
            ClassicAssert.AreEqual("Build#1", request.BuildName, "BuildName fails the get/set test");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void InitialiseRequestWithSessionSetsTheCorrectValues()
        {
            string sessionToken = "the session";
            DateTime now = DateTime.Now;
            BuildRequest request = new BuildRequest(sessionToken);
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
            BuildRequest request = new BuildRequest(sessionToken, projectName);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(request.Identifier), "Identifier was not set");
            ClassicAssert.AreEqual(Environment.MachineName, request.SourceName, "Source name doesn't match the machine name");
            ClassicAssert.AreEqual(sessionToken, request.SessionToken, "SessionToken doesn't match the input token");
            ClassicAssert.AreEqual(projectName, request.ProjectName, "ProjectName doesn't match the input project name");
            ClassicAssert.IsTrue((now <= request.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void ToStringSerialisesDefaultValues()
        {
            BuildRequest request = new BuildRequest();
            string actual = request.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<buildMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{2:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" source=\"{1}\" />",
                request.Identifier,
                request.SourceName,
                request.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }

        [Test]
        public void ToStringSerialisesAllValues()
        {
            BuildRequest request = new BuildRequest();
            request.Identifier = "identifier";
            request.ServerName = "serverName";
            request.SessionToken = "sessionToken";
            request.SourceName = "sourceName";
            request.Timestamp = DateTime.Now;
            request.BuildName = "Build#1";
            string actual = request.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<buildMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{4:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" server=\"{1}\" source=\"{2}\" session=\"{3}\" build=\"Build#1\" />",
                request.Identifier,
                request.ServerName,
                request.SourceName,
                request.SessionToken,
                request.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }
    }
}
