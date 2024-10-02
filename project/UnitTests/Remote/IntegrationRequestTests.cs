using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    [TestFixture]
    public class IntegrationRequestTests
    {
        #region Test methods
        #region GetHashCode()
        [Test]
        public void GetHashCodeReturnsAValidHasCode()
        {
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, 
                "Me",
                null);
            int expected = request.ToString().GetHashCode();
            int actual = request.GetHashCode();
            ClassicAssert.AreEqual(expected, actual);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
        #endregion
    }
}
