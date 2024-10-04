﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
    
    public class CruiseServerClientTests
    {
        private MockRepository mocks = new MockRepository(MockBehavior.Default);

        [Fact]
        public void ProcessMessageCorrectlyHandlesAValidMessage()
        {
            // Setup the messages
            ProjectRequest request = new ProjectRequest("123-45", "A test project");
            Response response = new Response(request);
            response.Result = ResponseResult.Success;

            // Initialises the mocks
            ICruiseServer server = mocks.Create<ICruiseServer>().Object;
            Mock.Get(server).Setup(_server => _server.ForceBuild(request)).Returns(response);

            // Run the actual test
            var manager = new ThoughtWorks.CruiseControl.Core.CruiseServerClient(server);
            string responseText = manager.ProcessMessage("ForceBuild", request.ToString());
            Assert.Equal(response.ToString(), responseText);
            Assert.Equal(response.ToString(), responseText);
            mocks.VerifyAll();
        }

        [Fact]
        public void ProcessMessageCorrectlyHandlesAnUnknownMessage()
        {
            // Initialises the mocks
            ICruiseServer server = mocks.Create<ICruiseServer>().Object;

            // Run the actual test
            var manager = new ThoughtWorks.CruiseControl.Core.CruiseServerClient(server);
            string responseText = manager.ProcessMessage("ForceBuild", "<garbage><data/></garbage>");
            Response response = ConvertXmlToResponse(responseText);
            Assert.Equal(ResponseResult.Failure, response.Result, "Result is unexpected");
        }

        [Fact]
        public void ProcessMessageCorrectlyHandlesAnUnknownAction()
        {
            // Setup the messages
            ProjectRequest request = new ProjectRequest("123-45", "A test project");

            // Initialises the mocks
            ICruiseServer server = mocks.Create<ICruiseServer>().Object;

            // Run the actual test
            var manager = new ThoughtWorks.CruiseControl.Core.CruiseServerClient(server);
            string responseText = manager.ProcessMessage("UnknownAction", request.ToString());
            Response response = ConvertXmlToResponse(responseText);
            Assert.Equal(ResponseResult.Failure, response.Result, "Result is unexpected");
        }

        /// <summary>
        /// Converts an XML instance into a response
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private Response ConvertXmlToResponse(string xml)
        {
            Response value = null;
            XmlSerializer serialiser = new XmlSerializer(typeof(Response));
            using (StringReader reader = new StringReader(xml))
            {
                value = serialiser.Deserialize(reader) as Response;
            }
            return value;
        }
    }
}
