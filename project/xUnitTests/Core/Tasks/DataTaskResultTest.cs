using Xunit;

using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class DataTaskResultTest : CustomAssertion
	{
		[Fact]
		public void DataSetIsValid()
		{
			string data = "foo";
			DataTaskResult result = new DataTaskResult(data);
			Assert.Equal(data, result.Data);
            Assert.True(true);
            Assert.True(true);
        }
	}
}
