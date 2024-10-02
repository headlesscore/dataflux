namespace ThoughtWorks.CruiseControl.UnitTests.IntegrationTests
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;

    public class DynamicValuesTests
    {
        #region Tests
        [Test]
        public void DynamicValuesHandlePrivateStringsWithPassword()
        {
            var document = EmbeddedData.LoadEmbeddedXml("DynamicValues.PrivateString.xml");
            var projectName = "Test Project";
            using (var harness = new CruiseServerHarness(document, projectName))
            {
                var status = IntegrationStatus.Unknown;
                harness.IntegrationCompleted += (o, e) =>
                {
                    status = e.Status;
                };
                harness.TriggerBuildAndWait(
                    projectName,
                    new NameValuePair("Password", "John's Password"));
                ClassicAssert.AreEqual(IntegrationStatus.Success, status);
                ClassicAssert.IsTrue(true);
                ClassicAssert.IsTrue(true);
            }
        }

        [Test]
        public void DynamicValuesHandlePrivateStringsWithoutPassword()
        {
            var document = EmbeddedData.LoadEmbeddedXml("DynamicValues.PrivateString.xml");
            var projectName = "Test Project";
            using (var harness = new CruiseServerHarness(document, projectName))
            {
                var status = IntegrationStatus.Unknown;
                harness.IntegrationCompleted += (o, e) =>
                {
                    status = e.Status;
                };
                harness.TriggerBuildAndWait(
                    projectName);
                ClassicAssert.AreEqual(IntegrationStatus.Success, status);
            }
        }
        #endregion
    }
}
