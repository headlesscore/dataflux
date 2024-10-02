namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;

    [TestFixture]
    public class WebClientFactoryTests
    {
        [Test]
        public void GeneratesStartsNewClient()
        {
            var factory = new WebClientFactory<DefaultWebClient>();
            var client = factory.Generate();
            ClassicAssert.IsInstanceOf<DefaultWebClient>(client);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
    }
}
