using System;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Configuration
{
	[TestFixture]
	public class BuildServerTest
	{
		[Test]
		public void CanBuildADisplayNameFromAServerUrl()
		{
			BuildServer server = new BuildServer("tcp://server:21234/blahblahblah.rem");
            ClassicAssert.AreEqual("server", server.DisplayName);			
		}

		[Test]
		public void ForHttpUrlsDisplayNameDisplaysTheEntireUrl()
		{
			BuildServer server = new BuildServer("http://one");
            ClassicAssert.AreEqual("http://one", server.DisplayName);
		}

		[Test]
		public void WhenThePortNumberIsNonDefaultThePortNumberIsDisplayed()
		{
			BuildServer server = new BuildServer("tcp://server:123/blahblahblah.rem");
            ClassicAssert.AreEqual("server:123", server.DisplayName);			
		}
		
		[Test]
		public void CanParseADisplayNameWithAPort()
		{
			BuildServer server = BuildServer.BuildFromRemotingDisplayName("server:123");
            ClassicAssert.AreEqual("tcp://server:123/CruiseManager.rem", server.Url);
		}

		[Test]
		public void CanParseADisplayNameWithoutAPort()
		{
			BuildServer server = BuildServer.BuildFromRemotingDisplayName("server");
            ClassicAssert.AreEqual("tcp://server:21234/CruiseManager.rem", server.Url);
		}

        [Test(Description = "Expected string in format server[:port]")]
        public void ThrowsWhenParsingAStringThatContainsMoreThanOneColon()
        {
            ClassicAssert.That(delegate { BuildServer.BuildFromRemotingDisplayName("tcp://server:123/blah.rem"); },
                        Throws.TypeOf<ApplicationException>());
        }

        [Test(Description = "Port number must be an integer")]
        public void ThrowsWhenParsingAStringWithNonNumericPortNumber()
        {
            ClassicAssert.That(delegate { BuildServer.BuildFromRemotingDisplayName("server:xxx"); },
                        Throws.TypeOf<ApplicationException>());
        }

        [Test(Description = "Extension transport must always define an extension name")]
        public void ThrowsWhenMissingExtension()
        {
            ClassicAssert.That(delegate { new BuildServer("http://test", BuildServerTransport.Extension, null, null); },
                        Throws.TypeOf<CCTrayLibException>());
        }

        [Test]
        public void SetExtensionName()
        {
            BuildServer newServer = new BuildServer();
            newServer.ExtensionName = "new extension";
            ClassicAssert.AreEqual("new extension", newServer.ExtensionName);
        }

        [Test]
        public void SetExtensionSettings()
        {
            BuildServer newServer = new BuildServer();
            newServer.ExtensionSettings = "new extension";
            ClassicAssert.AreEqual("new extension", newServer.ExtensionSettings);
        }

        [Test]
        public void SetTransport()
        {
            BuildServer newServer = new BuildServer();
            newServer.Transport = BuildServerTransport.Extension;
            ClassicAssert.AreEqual(BuildServerTransport.Extension, newServer.Transport);
        }
		
		[Test]
		public void TwoBuildServersAreEqualIfTheirUrlsAreTheSame()
		{
			BuildServer one = new BuildServer("http://one");
			BuildServer anotherOne = new BuildServer("http://one");
			BuildServer two = new BuildServer("http://two");

            ClassicAssert.AreEqual(one, one);
            ClassicAssert.AreEqual(one, anotherOne);
            ClassicAssert.AreEqual(anotherOne, one);
            ClassicAssert.IsFalse(one.Equals(two));
            ClassicAssert.IsFalse(two.Equals(one));
		}

	}
}
