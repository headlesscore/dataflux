namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using FluentAssertions;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;
    using ThoughtWorks.CruiseControl.Remote.Security;

    [TestFixture]
    public class ReadAuditResponseTests
    {
        [Test]
        public void InitialiseNewResponseSetsTheDefaultValues()
        {
            DateTime now = DateTime.Now;
            ReadAuditResponse response = new ReadAuditResponse();
            ClassicAssert.AreEqual(ResponseResult.Unknown, response.Result, "Result wasn't set to failure");
            ClassicAssert.IsTrue((now <= response.Timestamp), "Timestamp was not set");

            var records = new List<AuditRecord>();
            response.Records = records;
            ClassicAssert.AreSame(records, response.Records);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void InitialiseResponseFromRequestSetsTheDefaultValues()
        {
            DateTime now = DateTime.Now;
            ServerRequest request = new ServerRequest();
            ReadAuditResponse response = new ReadAuditResponse(request);
            ClassicAssert.AreEqual(ResponseResult.Unknown, response.Result, "Result wasn't set to failure");
            ClassicAssert.AreEqual(request.Identifier, response.RequestIdentifier, "RequestIdentifier wasn't set to the identifier of the request");
            ClassicAssert.IsTrue((now <= response.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void InitialiseResponseFromResponseSetsTheDefaultValues()
        {
            DateTime now = DateTime.Now;
            ReadAuditResponse response1 = new ReadAuditResponse();
            response1.Result = ResponseResult.Success;
            response1.RequestIdentifier = "original id";
            response1.Timestamp = DateTime.Now.AddMinutes(-1);
            ReadAuditResponse response2 = new ReadAuditResponse(response1);
            ClassicAssert.AreEqual(ResponseResult.Success, response2.Result, "Result wasn't set to failure");
            ClassicAssert.AreEqual("original id", response2.RequestIdentifier, "RequestIdentifier wasn't set to the identifier of the request");
            ClassicAssert.IsTrue((response1.Timestamp == response2.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void ToStringSerialisesDefaultValues()
        {
            ReadAuditResponse response = new ReadAuditResponse();
            string actual = response.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<readAuditResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{1:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" result=\"{0}\" />",
                response.Result,
                response.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }

        [Test]
        public void ToStringSerialisesAllValues()
        {
            ReadAuditResponse response = new ReadAuditResponse();
            response.ErrorMessages.Add(new ErrorMessage("Error 1"));
            response.ErrorMessages.Add(new ErrorMessage("Error 2"));
            response.RequestIdentifier = "request";
            response.Result = ResponseResult.Success;
            response.Timestamp = DateTime.Now;
            AuditRecord auditRecord = new AuditRecord();
            auditRecord.EventType = SecurityEvent.ViewAuditLog;
            auditRecord.Message = "testing";
            auditRecord.ProjectName = "test project";
            auditRecord.SecurityRight = SecurityRight.Deny;
            auditRecord.TimeOfEvent = response.Timestamp;
            auditRecord.UserName = "Test user";
            response.Records.Add(auditRecord);
            string actual = response.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<readAuditResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{2:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" result=\"{1}\">" +
                "<error>Error 1</error>" +
                "<error>Error 2</error>" +
                "<record time=\"{2:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" project=\"test project\" " +
                "user=\"Test user\" event=\"ViewAuditLog\" right=\"Deny\">" +
                "<message>testing</message>" + 
                "</record>" + 
                "</readAuditResponse>",
                response.RequestIdentifier,
                response.Result,
                response.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }
    }
}
