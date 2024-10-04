using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	
	public class EmailGroupTest
	{
		[Fact]
		public void ReadEmailGroupFromXmlUsingInvalidNotificationType()
		{
            Assert.True(delegate { NetReflector.Read(@"<group> name=""foo"" <notifications><NotificationType>bar</NotificationType></notifications>  </group>"); },
                        Throws.TypeOf<NetReflectorException>());
            Assert.True(delegate { NetReflector.Read(@"<group> name=""foo"" <notifications><NotificationType>bar</NotificationType></notifications>  </group>"); },
                        Throws.TypeOf<NetReflectorException>());
        }

		[Fact]
		public void ReadEmailGroupFromXmlUsingAlwaysNotificationType()
		{
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Always</NotificationType></notifications> </group>");
			Assert.Equal("foo", group.Name);
			Assert.True(group.HasNotification(EmailGroup.NotificationType.Always ));
		}

		[Fact]
		public void ReadEmailGroupFromXmlUsingChangeNotificationType()
		{
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Change</NotificationType></notifications> </group> ");
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Change));
        }

		[Fact]
		public void ReadEmailGroupFromXmlUsingFailedNotificationType()
		{
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Failed</NotificationType></notifications> </group>");
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Failed));
        }

        [Fact]
        public void ReadEmailGroupFromXmlUsingSuccessNotificationType()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Success</NotificationType></notifications> </group> ");
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Success));
        }

        [Fact]
        public void ReadEmailGroupFromXmlUsingFixedNotificationType()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Fixed</NotificationType></notifications> </group> ");
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Fixed));
        }

        [Fact]
        public void ReadEmailGroupFromXmlUsingExceptionNotificationType()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Exception</NotificationType></notifications> </group>");
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Exception));
        }


        [Fact]
        public void ReadEmailGroupFromXmlUsingMulipleNotificationTypes()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Failed</NotificationType><NotificationType>Fixed</NotificationType><NotificationType>Exception</NotificationType></notifications> </group> ");
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Exception));
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Fixed));
            Assert.True(group.HasNotification(EmailGroup.NotificationType.Failed));
            Assert.False(group.HasNotification(EmailGroup.NotificationType.Change));
        }


    
    }
}
