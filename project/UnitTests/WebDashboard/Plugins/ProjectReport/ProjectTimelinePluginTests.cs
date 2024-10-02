namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Plugins.ProjectReport
{
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
    using ThoughtWorks.CruiseControl.WebDashboard.MVC.Cruise;
    using ThoughtWorks.CruiseControl.WebDashboard.Plugins.ProjectReport;

    public class ProjectTimelinePluginTests
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
            var plugin = new ProjectTimelinePlugin(null);
            ClassicAssert.AreEqual("Project Timeline", plugin.LinkDescription);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void NamedActionsReturnsBothActions()
        {
            var actionInstantiator = this.mocks.Create<IActionInstantiator>(MockBehavior.Strict).Object;
            var action = this.mocks.Create<ICruiseAction>(MockBehavior.Strict).Object;
            Mock.Get(actionInstantiator).Setup(_actionInstantiator => _actionInstantiator.InstantiateAction(typeof(ProjectTimelineAction)))
                .Returns(action);

            var plugin = new ProjectTimelinePlugin(actionInstantiator);
            var actions = plugin.NamedActions;

            this.mocks.VerifyAll();
            ClassicAssert.AreEqual(2, actions.Length);
            ClassicAssert.IsInstanceOf<ImmutableNamedAction>(actions[0]);
            ClassicAssert.AreEqual(ProjectTimelineAction.TimelineActionName, actions[0].ActionName);
            ClassicAssert.AreSame(action, actions[0].Action);
            ClassicAssert.IsInstanceOf<ImmutableNamedAction>(actions[1]);
            ClassicAssert.AreEqual(ProjectTimelineAction.DataActionName, actions[1].ActionName);
            ClassicAssert.AreSame(action, actions[1].Action);
        }
        #endregion
    }
}
