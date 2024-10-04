using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class UserFilterTest
	{
		private UserFilter filter;

		// [SetUp]
		protected void CreateFilter()
		{
			filter = new UserFilter();			
		}

		[Fact]
		public void ShouldNotAcceptIfNoUserIsSpecified()
		{
			Assert.False(filter.Accept(new Modification()));
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldFilterSpecifiedUser()
		{
			Modification mod = new Modification();
			mod.UserName = "bob";

			filter.UserNames = new string[] { "bob" };
			Assert.True(filter.Accept(mod));
		}

		[Fact]
		public void LoadFromConfiguration()
		{
			filter = (UserFilter) NetReflector.Read(@"<userFilter><names><name>bob</name><name>perry</name></names></userFilter>");
			Assert.Equal(2, filter.UserNames.Length);
			Assert.Equal("bob", filter.UserNames[0]);
			Assert.Equal("perry", filter.UserNames[1]);
		}
	}
}
