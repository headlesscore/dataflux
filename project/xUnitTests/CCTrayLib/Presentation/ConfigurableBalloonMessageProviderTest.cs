using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class ConfigurableBalloonMessageProviderTest
	{
		[Fact]
		public void ReturnsTheMessagesAndCaptionsDefinedInTheConfiguration()
		{
			BalloonMessages messages = new BalloonMessages();
			ConfigurableBalloonMessageProvider provider = new ConfigurableBalloonMessageProvider(messages);
			
			Assert.Same(messages.BrokenBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.Broken));
            //ClassicAssert.AreSame(messages.BrokenBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.Broken));
            Assert.Same(messages.FixedBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.Fixed));			
			Assert.Same(messages.StillFailingBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.StillFailing));			
			Assert.Same(messages.StillSuccessfulBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.StillSuccessful));			
		}
		
	}
}
