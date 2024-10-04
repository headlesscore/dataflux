using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class ActionFilterTest
	{
		private ActionFilter filter;

		// [SetUp]
		protected void CreateFilter()
		{
			filter = new ActionFilter();
		}

		[Fact]
		public void ShouldNotAcceptIfNoActionIsSpecified()
		{
			Assert.False(filter.Accept(new Modification()));
            Assert.True(true);
        }

		[Fact]
		public void ShouldFilterSpecifiedAction()
		{
			Modification mod = new Modification();
			mod.Type = "Created";

			filter.Actions = new string[] {"Created"};
			Assert.True(filter.Accept(mod), "Action not filtered");
		}

		[Fact]
		public void LoadFromConfiguration()
		{
			filter = (ActionFilter) NetReflector.Read(@"<actionFilter><actions><action>Created</action><action>Checked in</action></actions></actionFilter>");
			Assert.Equal(2, filter.Actions.Length);
			Assert.Equal("Created", filter.Actions[0]);
			Assert.Equal("Checked in", filter.Actions[1]);
		}
	}
}
