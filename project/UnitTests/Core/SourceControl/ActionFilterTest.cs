using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class ActionFilterTest
	{
		private ActionFilter filter;

		[SetUp]
		protected void CreateFilter()
		{
			filter = new ActionFilter();
		}

		[Test]
		public void ShouldNotAcceptIfNoActionIsSpecified()
		{
			ClassicAssert.IsFalse(filter.Accept(new Modification()));
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldFilterSpecifiedAction()
		{
			Modification mod = new Modification();
			mod.Type = "Created";

			filter.Actions = new string[] {"Created"};
			ClassicAssert.IsTrue(filter.Accept(mod), "Action not filtered");
		}

		[Test]
		public void LoadFromConfiguration()
		{
			filter = (ActionFilter) NetReflector.Read(@"<actionFilter><actions><action>Created</action><action>Checked in</action></actions></actionFilter>");
			ClassicAssert.AreEqual(2, filter.Actions.Length);
			ClassicAssert.AreEqual("Created", filter.Actions[0]);
			ClassicAssert.AreEqual("Checked in", filter.Actions[1]);
		}
	}
}
