using System;
using System.Web;
using NUnit.Framework;
using Objection;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.BuildReport;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	//[TestFixture]
	public class CruiseObjectSourceInitializerTest
	{
		// See ToDo in CruiseObjectGiver Initializer - needs sorting out
//		[Test]
//		public void ShouldReturnAnObjectGiverThatCanGiveRequiredTypes()
//		{
////			HttpRequest request = new HttpRequest("foo", "http://foo/bar", "foo");
//			HttpContext context = new HttpContext(null);
//
//			ObjectGiverAndRegistrar objectGiver = new ObjectGiverAndRegistrar();
//			objectGiver.IgnoreNMockImplementations = true;
//
//			new CruiseObjectGiverInitializer(objectGiver).SetupObjectGiverForRequest(context);
//
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(HttpPathMapper)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(ServerQueryingBuildRetriever)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(CruiseManagerBuildNameRetriever)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(ConfigurationSettingsConfigGetter)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(PathMapperUsingHostName)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(DefaultBuildNameFormatter)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(ServerAggregatingCruiseManagerWrapper)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(SideBarViewBuilder)));
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectByType(typeof(TopControlsViewBuilder)));
//
//			// ToDo - test Plugins registered correctly
//		}

		// To Do - This still not runnable since ObjectGiverInitialiser still too tied in to ASP.NET - Will sort this out after 1.0 - MikeR
//		[Test]
//		public void ShouldSetupObjectGiverToBeAbleToCreateAlwaysRequiredPlugins()
//		{
//			HttpContext context = new HttpContext(null);
//			ObjectGiver objectGiver = new CruiseObjectGiverInitializer(new ManagableObjectGiver()).SetupObjectGiverForRequest(context);
//	
//			ClassicAssert.IsNotNull(objectGiver.GiveObjectById(LatestBuildReportProjectPlugin.ACTION_NAME));
//		}
}
	
	public class StubBuildPlugin : IPlugin
	{
		public INamedAction[] NamedActions
		{
			get { return new INamedAction[] { new ImmutableNamedAction("MyPlugin", null) }; }
		}

		public string LinkDescription
		{
			get { throw new NotImplementedException(); }
		}
	}
}
