namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Mercurial
{
	using Xunit;
	using System;

	/// <summary>
	/// Coverage test for <see cref="StubFileDirectoryDeleter"/>.
	/// </summary>
	
	public class StubFileDirectoryDeleterTest
	{
		#region Private Members

		private StubFileDirectoryDeleter sd;

		#endregion

		#region SetUp Method

		// [SetUp]
		public void SetUp()
		{
			sd = new StubFileDirectoryDeleter();
		}

		#endregion

		#region Tests

		[Fact]
		public void StubFileDirectoryDeleterCoverage()
		{
			sd.DeleteIncludingReadOnlyObjects("asdf");
		}

		#endregion
	}
}
