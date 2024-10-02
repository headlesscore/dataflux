using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	[TestFixture]
	public class DataTaskResultTest : CustomAssertion
	{
		[Test]
		public void DataSetIsValid()
		{
			string data = "foo";
			DataTaskResult result = new DataTaskResult(data);
			ClassicAssert.AreEqual(data, result.Data);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
	}
}
