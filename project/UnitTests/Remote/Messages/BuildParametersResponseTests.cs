﻿namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using FluentAssertions;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;
    using ThoughtWorks.CruiseControl.Remote.Parameters;

    [TestFixture]
    public class BuildParametersResponseTests
    {
        [Test]
        public void InitialiseNewResponseSetsTheDefaultValues()
        {
            DateTime now = DateTime.Now;
            BuildParametersResponse response = new BuildParametersResponse();
            var parameters = new List<ParameterBase>();
            response.Parameters = parameters;
            ClassicAssert.AreSame(parameters, response.Parameters);
            ClassicAssert.AreEqual(ResponseResult.Unknown, response.Result, "Result wasn't set to failure");
            ClassicAssert.IsTrue((now <= response.Timestamp), "Timestamp was not set");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void InitialiseResponseFromRequestSetsTheDefaultValues()
        {
            DateTime now = DateTime.Now;
            ServerRequest request = new ServerRequest();
            BuildParametersResponse response = new BuildParametersResponse(request);
            ClassicAssert.AreEqual(ResponseResult.Unknown, response.Result, "Result wasn't set to failure");
            ClassicAssert.AreEqual(request.Identifier, response.RequestIdentifier, "RequestIdentifier wasn't set to the identifier of the request");
            ClassicAssert.IsTrue((now <= response.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void InitialiseResponseFromResponseSetsTheDefaultValues()
        {
            DateTime now = DateTime.Now;
            BuildParametersResponse response1 = new BuildParametersResponse();
            response1.Result = ResponseResult.Success;
            response1.RequestIdentifier = "original id";
            response1.Timestamp = DateTime.Now.AddMinutes(-1);
            BuildParametersResponse response2 = new BuildParametersResponse(response1);
            ClassicAssert.AreEqual(ResponseResult.Success, response2.Result, "Result wasn't set to failure");
            ClassicAssert.AreEqual("original id", response2.RequestIdentifier, "RequestIdentifier wasn't set to the identifier of the request");
            ClassicAssert.IsTrue((response1.Timestamp == response2.Timestamp), "Timestamp was not set");
        }

        [Test]
        public void ToStringSerialisesDefaultValues()
        {
            BuildParametersResponse response = new BuildParametersResponse();
            string actual = response.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<buildParametersResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{1:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" result=\"{0}\" />",
                response.Result,
                response.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }

        [Test]
        public void ToStringSerialisesAllValues()
        {
            BuildParametersResponse response = new BuildParametersResponse();
            response.ErrorMessages.Add(new ErrorMessage("Error 1"));
            response.ErrorMessages.Add(new ErrorMessage("Error 2"));
            response.RequestIdentifier = "request";
            response.Result = ResponseResult.Success;
            response.Timestamp = DateTime.Now;
            response.Parameters.Add(new TextParameter("text"));
            response.Parameters.Add(new NumericParameter("numeric"));
            response.Parameters.Add(new SelectParameter("select"));
            string actual = response.ToString();
            string expected = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<buildParametersResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{2:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" identifier=\"{0}\" result=\"{1}\">" +
                "<error>Error 1</error>" +
                "<error>Error 2</error>" +
                "<parameter xsi:type=\"TextParameter\" name=\"text\" display=\"text\" />" +
                "<parameter xsi:type=\"NumericParameter\" name=\"numeric\" display=\"numeric\" />" +
                "<parameter xsi:type=\"SelectParameter\" name=\"select\" display=\"select\"><allowedValues /></parameter>" +
                "</buildParametersResponse>",
                response.RequestIdentifier,
                response.Result,
                response.Timestamp);

            XDocument.Parse(actual).Should().BeEquivalentTo(XDocument.Parse(expected));
        }
    }
}
