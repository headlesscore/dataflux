using System;
using System.Net;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture, Ignore("Test connect to external uris and hence are slow to run and do not work disconnected.")]
	public class HttpWrapperTest
	{
		private HttpWrapper httpWrapper;

		[SetUp]
		protected void SetUp()
		{
			httpWrapper = new HttpWrapper();			
		}

		[Test]
		public void TestValidUrlThatReturnsLastModified()
		{
			DateTime lastModTime = httpWrapper.GetLastModifiedTimeFor(new Uri(@"http://www.apache.org"), DateTime.MinValue);
			ClassicAssert.IsTrue(lastModTime > DateTime.MinValue);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void TestValidDynamicUrlThatDoesNotReturnLastModified()
		{
			DateTime lastModTime = httpWrapper.GetLastModifiedTimeFor(new Uri(@"http://confluence.public.thoughtworks.org/homepage.action"), DateTime.MinValue);
			ClassicAssert.AreEqual(DateTime.MinValue, lastModTime);
		}

		[Test]
		public void TestInvalidUrl()
		{
            ClassicAssert.That(delegate { httpWrapper.GetLastModifiedTimeFor(new Uri(@"http://wibble.wibble/"), DateTime.MinValue); },
                        Throws.TypeOf<WebException>());
		}

		[Test]
		public void TestLastModifiedIsNotChanged()
		{
			Uri urlToRequest = new Uri(@"http://www.apache.org/");
			DateTime lastModified = httpWrapper.GetLastModifiedTimeFor(urlToRequest, DateTime.MinValue);
			ClassicAssert.IsTrue(lastModified > DateTime.MinValue);

			DateTime notModified = httpWrapper.GetLastModifiedTimeFor(urlToRequest, lastModified);
			ClassicAssert.AreEqual(notModified, lastModified);
		}
	}
}
