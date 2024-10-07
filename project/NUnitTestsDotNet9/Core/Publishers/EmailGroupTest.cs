using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	[TestFixture]
	public class EmailGroupTest
	{
		[Test]
		public void ReadEmailGroupFromXmlUsingInvalidNotificationType()
		{
            ClassicAssert.That(delegate { NetReflector.Read(@"<group> name=""foo"" <notifications><NotificationType>bar</NotificationType></notifications>  </group>"); },
                        Throws.TypeOf<NetReflectorException>());
            ClassicAssert.That(delegate { NetReflector.Read(@"<group> name=""foo"" <notifications><NotificationType>bar</NotificationType></notifications>  </group>"); },
                        Throws.TypeOf<NetReflectorException>());
        }

		[Test]
		public void ReadEmailGroupFromXmlUsingAlwaysNotificationType()
		{
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Always</NotificationType></notifications> </group>");
			ClassicAssert.AreEqual("foo", group.Name);
			ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Always ));
		}

		[Test]
		public void ReadEmailGroupFromXmlUsingChangeNotificationType()
		{
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Change</NotificationType></notifications> </group> ");
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Change));
        }

		[Test]
		public void ReadEmailGroupFromXmlUsingFailedNotificationType()
		{
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Failed</NotificationType></notifications> </group>");
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Failed));
        }

        [Test]
        public void ReadEmailGroupFromXmlUsingSuccessNotificationType()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Success</NotificationType></notifications> </group> ");
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Success));
        }

        [Test]
        public void ReadEmailGroupFromXmlUsingFixedNotificationType()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Fixed</NotificationType></notifications> </group> ");
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Fixed));
        }

        [Test]
        public void ReadEmailGroupFromXmlUsingExceptionNotificationType()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Exception</NotificationType></notifications> </group>");
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Exception));
        }


        [Test]
        public void ReadEmailGroupFromXmlUsingMulipleNotificationTypes()
        {
            EmailGroup group = (EmailGroup)NetReflector.Read(@"<group name=""foo""> <notifications><NotificationType>Failed</NotificationType><NotificationType>Fixed</NotificationType><NotificationType>Exception</NotificationType></notifications> </group> ");
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Exception));
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Fixed));
            ClassicAssert.IsTrue(group.HasNotification(EmailGroup.NotificationType.Failed));
            ClassicAssert.IsFalse(group.HasNotification(EmailGroup.NotificationType.Change));
        }


    
    }
}
