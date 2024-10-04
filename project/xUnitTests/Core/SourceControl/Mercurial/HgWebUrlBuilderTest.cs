namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Mercurial
{
	using Xunit;
    
    using ThoughtWorks.CruiseControl.Core;
	using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Mercurial;

	/// <summary>
	/// Basic test for the HgWebUrlBuilder
	/// </summary>
	
	public class HgWebUrlBuilderTest
	{
		#region Tests

		/// <summary>
		/// Ensure that a valid url is built with a given url.
		/// </summary>
		[Fact]
		public void ShouldBuildValidUrls()
		{
			HgWebUrlBuilder hgweb = new HgWebUrlBuilder();
			hgweb.Url = "http://selenic.com/hg/index.cgi/";
			Modification[] modifications = new Modification[] {new Modification()};
			modifications[0].Version = "4a064e1977f8";
			hgweb.SetupModification(modifications);

			Assert.False(modifications[0].Url is null);
            Assert.True(modifications[0].Url == "http://selenic.com/hg/index.cgi/rev/4a064e1977f8");
        }

		#endregion
	}
}
