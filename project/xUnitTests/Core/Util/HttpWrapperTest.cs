using System;
using System.Net;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    //[TestFixture, Ignore("Test connect to external uris and hence are slow to run and do not work disconnected.")]
	public class HttpWrapperTest
	{
		private HttpWrapper httpWrapper;

		// [SetUp]
		protected void SetUp()
		{
			httpWrapper = new HttpWrapper();			
		}

		[Fact]
		public void TestValidUrlThatReturnsLastModified()
		{
			DateTime lastModTime = httpWrapper.GetLastModifiedTimeFor(new Uri(@"http://www.apache.org"), DateTime.MinValue);
			Assert.True(lastModTime > DateTime.MinValue);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void TestValidDynamicUrlThatDoesNotReturnLastModified()
		{
			DateTime lastModTime = httpWrapper.GetLastModifiedTimeFor(new Uri(@"http://confluence.public.thoughtworks.org/homepage.action"), DateTime.MinValue);
			Assert.Equal(DateTime.MinValue, lastModTime);
		}

		[Fact]
		public void TestInvalidUrl()
		{
            Assert.Throws<WebException>(delegate { httpWrapper.GetLastModifiedTimeFor(new Uri(@"http://wibble.wibble/"), DateTime.MinValue); });
		}

		[Fact]
		public void TestLastModifiedIsNotChanged()
		{
			Uri urlToRequest = new Uri(@"http://www.apache.org/");
			DateTime lastModified = httpWrapper.GetLastModifiedTimeFor(urlToRequest, DateTime.MinValue);
			Assert.True(lastModified > DateTime.MinValue);

			DateTime notModified = httpWrapper.GetLastModifiedTimeFor(urlToRequest, lastModified);
			Assert.Equal(notModified, lastModified);
		}
	}
}
