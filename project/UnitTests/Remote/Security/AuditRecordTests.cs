using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using ThoughtWorks.CruiseControl.Remote.Security;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Security
{
    [TestFixture]
    public class AuditRecordTests
    {
        [Test]
        public void SetGetAllProperties()
        {
            AuditRecord record = new AuditRecord();
            record.EventType = SecurityEvent.ViewAuditLog;
            ClassicAssert.AreEqual(SecurityEvent.ViewAuditLog, record.EventType, "EventType get/set mismatch");
            record.Message = "Test Message";
            ClassicAssert.AreEqual("Test Message", record.Message, "Message get/set mismatch");
            record.ProjectName = "Test Project";
            ClassicAssert.AreEqual("Test Project", record.ProjectName, "ProjectName get/set mismatch");
            record.SecurityRight = SecurityRight.Allow;
            ClassicAssert.AreEqual(SecurityRight.Allow, record.SecurityRight, "SecurityRight get/set mismatch");
            record.TimeOfEvent = DateTime.Today;
            ClassicAssert.AreEqual(DateTime.Today, record.TimeOfEvent, "TimeOfEvent get/set mismatch");
            record.UserName = "Test User";
            ClassicAssert.AreEqual("Test User", record.UserName, "UserName get/set mismatch");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
    }
}
