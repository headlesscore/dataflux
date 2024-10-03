using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class CCTrayMultiSettingsFormTest
	{
		[Fact]
        
		public void ShouldCloneConfigurationAndOnlyBindToTheClone()
		{
			var existingConfiguration = new Mock<ICCTrayMultiConfiguration>(MockBehavior.Strict);
			CCTrayMultiConfiguration clonedConfiguration = new CCTrayMultiConfiguration(null, null, null);
			existingConfiguration.Setup(_configuration => _configuration.Clone()).Returns(clonedConfiguration).Verifiable();
			
			NullReferenceException nullReference = null;
			try
			{
				new CCTrayMultiSettingsForm((ICCTrayMultiConfiguration)existingConfiguration.Object);
			}
			catch (NullReferenceException e)
			{
				nullReference = e;
			}

			// As we are using a Strict mock, incorrect calls to the existing configuration 
			// will be caught by the verify.
			existingConfiguration.Verify();
			Assert.False(nullReference is null,
				$"There was a null reference exception not related to using existing configuration:\n{nullReference}");
        }
	}
}
