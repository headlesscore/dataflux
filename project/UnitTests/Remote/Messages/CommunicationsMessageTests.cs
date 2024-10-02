namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class CommunicationsMessageTests
    {
        #region Tests
        [Test]
        public void ChannelCanBeSetAndRetrieved()
        {
            var channelInfo = new object();
            var message = new ServerRequest();
            message.ChannelInformation = channelInfo;
            ClassicAssert.AreSame(channelInfo, message.ChannelInformation);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
    }
}
