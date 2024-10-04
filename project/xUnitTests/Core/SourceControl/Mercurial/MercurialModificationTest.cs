namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Mercurial
{
	using Xunit;
	using Exortech.NetReflector;
	using System;
	using ThoughtWorks.CruiseControl.Core;
	using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Mercurial;
    

    /// <summary>
    /// Test fixture for the <see cref="MercurialModification"/> class.
    /// </summary>
	
	public class MercurialModificationTest
	{
		#region Tests

		[Fact]
		public void ShouldConvertToModification()
		{
			var hgMod = new MercurialModification
			{
				ChangeNumber = 100,
				Comment = Guid.NewGuid().ToString(),
				EmailAddress = Guid.NewGuid().ToString(),
				FileName = Guid.NewGuid().ToString(),
				FolderName = Guid.NewGuid().ToString(),
				ModifiedTime = DateTime.UtcNow,
				UserName = Guid.NewGuid().ToString(),
				Version = Guid.NewGuid().ToString()
			};

			var ccnetMod = (Modification) hgMod;

            Assert.True(true);
            Assert.True(ccnetMod.ChangeNumber == hgMod.ChangeNumber.ToString());
			Assert.True(ccnetMod.Comment == hgMod.Comment);
			Assert.True(ccnetMod.EmailAddress == hgMod.EmailAddress);
			Assert.True(ccnetMod.FileName == hgMod.FileName);
			Assert.True(ccnetMod.FolderName == hgMod.FolderName);
			Assert.True(ccnetMod.ModifiedTime == hgMod.ModifiedTime);
			Assert.True(ccnetMod.UserName == hgMod.UserName);
			Assert.True(ccnetMod.Version == hgMod.Version);
		}

		#endregion
	}
}
