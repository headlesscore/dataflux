using System;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    [TestFixture]
    public class ServerRequestTests
    {
        [Test]
        public void GetSetAllPropertiesWorks()
        {
            ServerRequest request = new ServerRequest();
            request.Identifier = "new id";
            ClassicAssert.AreEqual("new id", request.Identifier, "Identifier fails the get/set test");
            request.ServerName = "new server";
            ClassicAssert.AreEqual("new server", request.ServerName, "ServerName fails the get/set test");
            request.SessionToken = "new session";
            ClassicAssert.AreEqual("new session", request.SessionToken, "SessionToken fails the get/set test");
            request.SourceName = "new source";
            ClassicAssert.AreEqual("new source", request.SourceName, "SourceName fails the get/set test");
            DateTime now = DateTime.Now;
            request.Timestamp = now;
            ClassicAssert.AreEqual(now, request.Timestamp, "Timestamp fails the get/set test");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void InitialiseNewRequestSetsTheDefaultValues()
        {
            DateTime now = DateTime.Now;
            ServerRequest request = new ServerRequest();
            ClassicAssert.IsFalse(string.IsNullOrEmpty(request.Identifier), "Identifier was not set");
            ClassicAssert.AreEqual(Environment.MachineName, request.SourceName, "Source name doesn't match the machine name");
            ClassicAssert.IsTrue((now <= request.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void InitialiseRequestWithSessionSetsTheCorrectValues()
        {
            string sessionToken = "the session";
            DateTime now = DateTime.Now;
            ServerRequest request = new ServerRequest(sessionToken);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(request.Identifier), "Identifier was not set");
            ClassicAssert.AreEqual(Environment.MachineName, request.SourceName, "Source name doesn't match the machine name");
            ClassicAssert.AreEqual(sessionToken, request.SessionToken, "SessionToken doesn't match the input token");
            ClassicAssert.IsTrue((now <= request.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void EqualsMatchesRequestWithTheSameIdentifier()
        {
            ServerRequest request1 = new ServerRequest();
            ServerRequest request2 = new ServerRequest();
            request1.Identifier = request2.Identifier;
            ClassicAssert.IsTrue(request1.Equals(request2));
        }

        [Test]
        public void EqualsDoesNotMatchesRequestWithDifferentIdentifier()
        {
            ServerRequest request1 = new ServerRequest();
            ServerRequest request2 = new ServerRequest();
            ClassicAssert.IsFalse(request1.Equals(request2));
        }

        [Test]
        public void EqualsDoesNotMatchDifferentTypes()
        {
            var request = new ServerRequest();
            var different = new object();
            ClassicAssert.IsFalse(request.Equals(different));
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfIdentifier()
        {
            ServerRequest request = new ServerRequest();
            ClassicAssert.AreEqual(request.Identifier.GetHashCode(), request.GetHashCode());
        }

        [Test]
        public void ToStringSerialisesDefaultValues()
        {
            ServerRequest request = new ServerRequest();
            string actual = request.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<serverMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{2:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" source=\"{1}\" />",
                request.Identifier,
                request.SourceName,
                request.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }

        [Test]
        public void ToStringSerialisesAllValues()
        {
            ServerRequest request = new ServerRequest();
            request.Identifier = "identifier";
            request.ServerName = "serverName";
            request.SessionToken = "sessionToken";
            request.SourceName = "sourceName";
            request.Timestamp = DateTime.Now;
            string actual = request.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<serverMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{4:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" server=\"{1}\" source=\"{2}\" session=\"{3}\" />",
                request.Identifier,
                request.ServerName,
                request.SourceName,
                request.SessionToken,
                request.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }
    }
}
