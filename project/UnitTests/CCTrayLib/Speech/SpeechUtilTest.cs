/*
 * Created by SharpDevelop.
 * User: sdonie
 * Date: 2/27/2009
 * Time: 1:03 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Speech;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Speech
{
	[TestFixture]
	public class SpeechUtilTest
	{
		[Test]
		public void TestMakeProjectNameSpeechFriendly()
		{
			ClassicAssert.AreEqual("expected",SpeechUtil.makeProjectNameMoreSpeechFriendly("expected"));
            ClassicAssert.AreEqual("expected", SpeechUtil.makeProjectNameMoreSpeechFriendly("expected"));
            ClassicAssert.AreEqual("the One True Project",SpeechUtil.makeProjectNameMoreSpeechFriendly("theOneTrueProject"));
			ClassicAssert.AreEqual("Project 1",SpeechUtil.makeProjectNameMoreSpeechFriendly("Project_1"));
			ClassicAssert.AreEqual("A Project With First Word A Single Letter",SpeechUtil.makeProjectNameMoreSpeechFriendly("AProjectWithFirstWordASingleLetter"));
			ClassicAssert.AreEqual("a Project With First Word A Single Letter",SpeechUtil.makeProjectNameMoreSpeechFriendly("aProjectWithFirstWordASingleLetter"));
			ClassicAssert.AreEqual("A Project With Some Underscores",SpeechUtil.makeProjectNameMoreSpeechFriendly("AProjectWith_Some_Underscores"));
			ClassicAssert.AreEqual("A Project With some dashes",SpeechUtil.makeProjectNameMoreSpeechFriendly("AProjectWith-some-dashes"));
			ClassicAssert.AreEqual("",SpeechUtil.makeProjectNameMoreSpeechFriendly(""));
		}
		
		[Test]
		public void TestWhetherWeShouldSpeak(){
			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,true,false));
			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.Fixed,true,false));
			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,true,true));
			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.Fixed,true,true));

			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.Broken,false,true));
			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,false,true));
			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.Broken,true,true));
			ClassicAssert.IsTrue(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,true,true));

			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.Broken,true,false));
			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,true,false));
			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.Broken,false,false));
			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.StillFailing,false,false));
			
			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,false,true));
			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.Fixed,false,true));
			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.StillSuccessful,false,false));
			ClassicAssert.IsFalse(SpeechUtil.shouldSpeak(BuildTransition.Fixed,false,false));
		}
	}
}
