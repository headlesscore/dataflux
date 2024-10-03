#if !DISABLE_COM
using System;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Speech;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class SpeechSettingsControlTest
	{

        [Fact]
        public void CanBindToDefaultConfiguration()
        {
        	SpeechSettingsControl control = new SpeechSettingsControl();

        	SpeechConfiguration configuration = new SpeechConfiguration();
            control.BindSpeechControls(configuration);
        }
	}
}
#endif
