namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Mercurial
{
	using NUnit.Framework;
	using Exortech.NetReflector;
	using System;
	using ThoughtWorks.CruiseControl.Core;
	using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Mercurial;
    using NUnit.Framework.Legacy;

    /// <summary>
    /// Test fixture for the <see cref="MercurialModification"/> class.
    /// </summary>
	[TestFixture]
	public class MercurialModificationTest
	{
		#region Tests

		[Test]
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

            ClassicAssert.IsTrue(true);
            ClassicAssert.That(ccnetMod.ChangeNumber, Is.EqualTo(hgMod.ChangeNumber.ToString()));
			ClassicAssert.That(ccnetMod.Comment, Is.EqualTo(hgMod.Comment));
			ClassicAssert.That(ccnetMod.EmailAddress, Is.EqualTo(hgMod.EmailAddress));
			ClassicAssert.That(ccnetMod.FileName, Is.EqualTo(hgMod.FileName));
			ClassicAssert.That(ccnetMod.FolderName, Is.EqualTo(hgMod.FolderName));
			ClassicAssert.That(ccnetMod.ModifiedTime, Is.EqualTo(hgMod.ModifiedTime));
			ClassicAssert.That(ccnetMod.UserName, Is.EqualTo(hgMod.UserName));
			ClassicAssert.That(ccnetMod.Version, Is.EqualTo(hgMod.Version));
		}

		#endregion
	}
}
