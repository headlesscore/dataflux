using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	[TestFixture]
	public class ConfigurableBalloonMessageProviderTest
	{
		[Test]
		public void ReturnsTheMessagesAndCaptionsDefinedInTheConfiguration()
		{
			BalloonMessages messages = new BalloonMessages();
			ConfigurableBalloonMessageProvider provider = new ConfigurableBalloonMessageProvider(messages);
			
			ClassicAssert.AreSame(messages.BrokenBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.Broken));
            //ClassicAssert.AreSame(messages.BrokenBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.Broken));
            ClassicAssert.AreSame(messages.FixedBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.Fixed));			
			ClassicAssert.AreSame(messages.StillFailingBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.StillFailing));			
			ClassicAssert.AreSame(messages.StillSuccessfulBuildMessage, provider.GetCaptionAndMessageForBuildTransition(BuildTransition.StillSuccessful));			
		}
		
	}
}
