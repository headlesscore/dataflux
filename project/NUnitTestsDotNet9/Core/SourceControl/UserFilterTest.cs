using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class UserFilterTest
	{
		private UserFilter filter;

		[SetUp]
		protected void CreateFilter()
		{
			filter = new UserFilter();			
		}

		[Test]
		public void ShouldNotAcceptIfNoUserIsSpecified()
		{
			ClassicAssert.IsFalse(filter.Accept(new Modification()));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldFilterSpecifiedUser()
		{
			Modification mod = new Modification();
			mod.UserName = "bob";

			filter.UserNames = new string[] { "bob" };
			ClassicAssert.IsTrue(filter.Accept(mod));
		}

		[Test]
		public void LoadFromConfiguration()
		{
			filter = (UserFilter) NetReflector.Read(@"<userFilter><names><name>bob</name><name>perry</name></names></userFilter>");
			ClassicAssert.AreEqual(2, filter.UserNames.Length);
			ClassicAssert.AreEqual("bob", filter.UserNames[0]);
			ClassicAssert.AreEqual("perry", filter.UserNames[1]);
		}
	}
}
