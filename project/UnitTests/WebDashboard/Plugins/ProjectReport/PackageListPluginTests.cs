namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Plugins.ProjectReport
{
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
    using ThoughtWorks.CruiseControl.WebDashboard.MVC.Cruise;
    using ThoughtWorks.CruiseControl.WebDashboard.Plugins.ProjectReport;

    public class PackageListPluginTests
    {
        #region Private fields
        private MockRepository mocks;
        #endregion

        #region Setup
        [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }
        #endregion

        #region Tests
        [Test]
        public void DescriptionIsCorrect()
        {
            var plugin = new PackageListPlugin(null);
            ClassicAssert.AreEqual("Package List", plugin.LinkDescription);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void NamedActionsReturnsSingleAction()
        {
            var actionInstantiator = this.mocks.Create<IActionInstantiator>(MockBehavior.Strict).Object;
            var action = this.mocks.Create<ICruiseAction>(MockBehavior.Strict).Object;
            Mock.Get(actionInstantiator).Setup(_actionInstantiator => _actionInstantiator.InstantiateAction(typeof(PackageListAction)))
                .Returns(action);

            var plugin = new PackageListPlugin(actionInstantiator);
            var actions = plugin.NamedActions;

            this.mocks.VerifyAll();
            ClassicAssert.AreEqual(1, actions.Length);
            ClassicAssert.IsInstanceOf<ImmutableNamedAction>(actions[0]);
            ClassicAssert.AreEqual(PackageListAction.ActionName, actions[0].ActionName);
            ClassicAssert.AreSame(action, actions[0].Action);
        }
        #endregion
    }
}
