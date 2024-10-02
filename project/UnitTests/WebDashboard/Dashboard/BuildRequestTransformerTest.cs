using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class BuildRequestTransformerTest
	{
		[Test]
		public void ShouldGetBuildLogAndReturnResultOfDelegateTransformer()
		{
			var buildRetrieverMock = new Mock<IBuildRetriever>();
			var delegateTransformerMock = new Mock<IMultiTransformer>();

			BuildRequestTransformer requestTransformer = new BuildRequestTransformer((IBuildRetriever) buildRetrieverMock.Object, (IMultiTransformer) delegateTransformerMock.Object);

			DefaultBuildSpecifier buildSpecifier = new DefaultBuildSpecifier(new DefaultProjectSpecifier(new DefaultServerSpecifier("myServer"), "myProject"), "myBuild");

			Build build = new Build(buildSpecifier, "logContents");

			buildRetrieverMock.Setup(retriever => retriever.GetBuild(buildSpecifier, null)).Returns(build).Verifiable();

			string[] fileNames = new string[] { "file1", "file2" };

			delegateTransformerMock.Setup(transformer => transformer.Transform("logContents", fileNames, null)).Returns("transformed").Verifiable();

			ClassicAssert.AreEqual("transformed", requestTransformer.Transform(buildSpecifier, fileNames, null, null));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            buildRetrieverMock.Verify();
			delegateTransformerMock.Verify();
		}
	}
}
