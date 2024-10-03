using System;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.X10;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
    public class X10SettingsControlTest
    {
        [Fact]

        public void CanBindToDefaultConfiguration()
        {
            X10SettingsControl control = new X10SettingsControl();

            X10Configuration configuration = new X10Configuration();
            control.BindX10Controls(configuration);
        }


    }
}
