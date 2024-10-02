namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;

    [TestFixture]
    public class NameValuePairListSerialiserFactoryTests
    {
        #region Tests
        [Test]
        public void CreateGeneratesNewSerialiser()
        {
            var factory = new NameValuePairListSerialiserFactory();
            var serialiser = factory.Create(null, null);
            ClassicAssert.IsInstanceOf<NameValuePairSerialiser>(serialiser);
            var actualSerialiser = serialiser as NameValuePairSerialiser;
            ClassicAssert.IsTrue(actualSerialiser.IsList);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
    }
}
