using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.MVC
{
	[TestFixture]
	public class NameValueCollectionRequestTest
	{
		[Test]
		public void ShouldReturnFileNameWithoutExtension()
		{
			NameValueCollectionRequest request = new NameValueCollectionRequest(null, null, "/ccnet/file1.aspx", null, null);
			ClassicAssert.AreEqual("file1", request.FileNameWithoutExtension);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);

            request = new NameValueCollectionRequest(null, null, "/file2.aspx", null, null);
			ClassicAssert.AreEqual("file2", request.FileNameWithoutExtension);

            request = new NameValueCollectionRequest(null, null, "/ccnet/file3", null, null);
			ClassicAssert.AreEqual("file3", request.FileNameWithoutExtension);

            request = new NameValueCollectionRequest(null, null, "/file4", null, null);
			ClassicAssert.AreEqual("file4", request.FileNameWithoutExtension);

            request = new NameValueCollectionRequest(null, null, "/", null, null);
			ClassicAssert.AreEqual("", request.FileNameWithoutExtension);
		}
	}
}
