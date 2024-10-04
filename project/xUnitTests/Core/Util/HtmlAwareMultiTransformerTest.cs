using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class HtmlAwareMultiTransformerTest
	{
		[Fact]
		public void ShouldDelegateForEachFileAndSeparateWithLineBreaks()
		{
			var delegateMock = new Mock<ITransformer>();
			HtmlAwareMultiTransformer transformer = new HtmlAwareMultiTransformer((ITransformer) delegateMock.Object);

			string input = "myinput";

			delegateMock.Setup(t => t.Transform(input, "xslFile1", null)).Returns(@"<p>MyFirstOutput<p>").Verifiable();
			delegateMock.Setup(t => t.Transform(input, "xslFile2", null)).Returns(@"<p>MySecondOutput<p>").Verifiable();

			Assert.Equal(@"<p>MyFirstOutput<p><p>MySecondOutput<p>", transformer.Transform(input, new string[] { "xslFile1", "xslFile2" }, null));
            Assert.True(true);
            Assert.True(true);
            delegateMock.Verify();
		}
	}
}
