/*
 * Created by SharpDevelop.
 * User: sdonie
 * Date: 2/27/2009
 * Time: 1:03 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Speech;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Speech
{
	public class SpeechUtilTest
	{
		[Fact]
		public void TestMakeProjectNameSpeechFriendly()
		{
			Assert.Equal("expected",SpeechUtil.makeProjectNameMoreSpeechFriendly("expected"));
            Assert.Equal("expected", SpeechUtil.makeProjectNameMoreSpeechFriendly("expected"));
            Assert.Equal("the One True Project",SpeechUtil.makeProjectNameMoreSpeechFriendly("theOneTrueProject"));
			Assert.Equal("Project 1",SpeechUtil.makeProjectNameMoreSpeechFriendly("Project_1"));
			Assert.Equal("A Project With First Word A Single Letter",SpeechUtil.makeProjectNameMoreSpeechFriendly("AProjectWithFirstWordASingleLetter"));
			Assert.Equal("a Project With First Word A Single Letter",SpeechUtil.makeProjectNameMoreSpeechFriendly("aProjectWithFirstWordASingleLetter"));
			Assert.Equal("A Project With Some Underscores",SpeechUtil.makeProjectNameMoreSpeechFriendly("AProjectWith_Some_Underscores"));
			Assert.Equal("A Project With some dashes",SpeechUtil.makeProjectNameMoreSpeechFriendly("AProjectWith-some-dashes"));
			Assert.Equal("",SpeechUtil.makeProjectNameMoreSpeechFriendly(""));
		}
		
		[Fact]
		public void TestWhetherWeShouldSpeak(){
			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,true,false));
			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.Fixed,true,false));
			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,true,true));
			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.Fixed,true,true));

			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.Broken,false,true));
			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,false,true));
			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.Broken,true,true));
			Assert.True(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,true,true));

			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.Broken,true,false));
			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,true,false));
			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.Broken,false,false));
			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,false,false));
			
			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,false,true));
			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.Fixed,false,true));
			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,false,false));
			Assert.False(SpeechUtil.shouldSpeak(BuildTransition.Fixed,false,false));
		}
	}
}
