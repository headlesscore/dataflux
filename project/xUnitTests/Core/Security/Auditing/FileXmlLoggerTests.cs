﻿using Xunit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Core.Security.Auditing;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Security;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security.Auditing
{
    
    public class FileXmlLoggerTests
    {
        [Fact]
        public void GetSetAllProperties()
        {
            FileXmlLogger logger = new FileXmlLogger();
            string fileName = "LogFile.xml";
            logger.AuditFileLocation = fileName;
            Assert.Equal(fileName, logger.AuditFileLocation);

            logger.LogFailureEvents = false;
            Assert.False(logger.LogFailureEvents, "LogFailureEvents not correctly set");
            logger.LogFailureEvents = true;
            Assert.True(logger.LogFailureEvents, "LogFailureEvents not correctly set");

            logger.LogSuccessfulEvents = false;
            Assert.False(logger.LogSuccessfulEvents, "LogSuccessfulEvents not correctly set");
            logger.LogSuccessfulEvents = true;
            Assert.True(logger.LogSuccessfulEvents, "LogSuccessfulEvents not correctly set");
        }

        [Fact]
        public void LogAllowEventWithSuccessOn()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "Log1.xml");
            if (File.Exists(logFile)) File.Delete(logFile);

            FileXmlLogger logger = new FileXmlLogger();
            logger.AuditFileLocation = logFile;
            logger.LogSuccessfulEvents = true;

            logger.LogEvent("A project", "A user", SecurityEvent.ForceBuild, SecurityRight.Allow, "A message");
            Assert.True(File.Exists(logFile), "Audit log not generated");
            string actual = File.ReadAllText(logFile);
            string expected = "<event>" +
                    "<dateTime>[^<]*</dateTime>" + 
                    "<project>A project</project>" +
                    "<user>A user</user>" +
                    "<type>ForceBuild</type>" +
                    "<outcome>Allow</outcome>" + 
                    "<message>A message</message>" + 
                "</event>";
            RunRegexTest(expected, actual);
        }

        [Fact]
        public void LogAllowEventWithSuccessOff()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "Log2.xml");
            if (File.Exists(logFile)) File.Delete(logFile);

            FileXmlLogger logger = new FileXmlLogger();
            logger.AuditFileLocation = logFile;
            logger.LogSuccessfulEvents = false;

            logger.LogEvent("A project", "A user", SecurityEvent.ForceBuild, SecurityRight.Allow, "A message");
            Assert.False(File.Exists(logFile), "Audit log was generated");
            
        }

        [Fact]
        public void LogDenyEventWithFailureOn()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "Log1.xml");
            if (File.Exists(logFile)) File.Delete(logFile);

            FileXmlLogger logger = new FileXmlLogger();
            logger.AuditFileLocation = logFile;
            logger.LogFailureEvents = true;

            logger.LogEvent("A project", "A user", SecurityEvent.ForceBuild, SecurityRight.Deny, "A message");
            Assert.True(File.Exists(logFile), "Audit log not generated");
            string actual = File.ReadAllText(logFile);
            string expected = "<event>" +
                    "<dateTime>[^<]*</dateTime>" +
                    "<project>A project</project>" +
                    "<user>A user</user>" +
                    "<type>ForceBuild</type>" +
                    "<outcome>Deny</outcome>" +
                    "<message>A message</message>" +
                "</event>";
            RunRegexTest(expected, actual);
        }

        [Fact]
        public void LogDenyEventWithFailureOff()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "Log2.xml");
            if (File.Exists(logFile)) File.Delete(logFile);

            FileXmlLogger logger = new FileXmlLogger();
            logger.AuditFileLocation = logFile;
            logger.LogFailureEvents = false;

            logger.LogEvent("A project", "A user", SecurityEvent.ForceBuild, SecurityRight.Deny, "A message");
            Assert.False(File.Exists(logFile), "Audit log was generated");
        }

        [Fact]
        public void LogInheritEvent()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "Log1.xml");
            if (File.Exists(logFile)) File.Delete(logFile);

            FileXmlLogger logger = new FileXmlLogger();
            logger.AuditFileLocation = logFile;

            logger.LogEvent("A project", "A user", SecurityEvent.ForceBuild, SecurityRight.Inherit, "A message");
            Assert.True(File.Exists(logFile), "Audit log not generated");
            string actual = File.ReadAllText(logFile);
            string expected = "<event>" +
                    "<dateTime>[^<]*</dateTime>" +
                    "<project>A project</project>" +
                    "<user>A user</user>" +
                    "<type>ForceBuild</type>" +
                    "<message>A message</message>" +
                "</event>";
            RunRegexTest(expected, actual);
        }

        [Fact]
        public void LogEmptyEvent()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "Log1.xml");
            if (File.Exists(logFile)) File.Delete(logFile);

            FileXmlLogger logger = new FileXmlLogger();
            logger.AuditFileLocation = logFile;

            logger.LogEvent(null, null, SecurityEvent.ForceBuild, SecurityRight.Inherit, null);
            Assert.True(File.Exists(logFile), "Audit log not generated");
            string actual = File.ReadAllText(logFile);
            string expected = "<event>" +
                    "<dateTime>[^<]*</dateTime>" +
                    "<type>ForceBuild</type>" +
                "</event>";
            RunRegexTest(expected, actual);
        }

        private void RunRegexTest(string expected, string actual)
        {
            Regex test = new Regex(expected);
            if (!test.IsMatch(actual))
            {
                Assert.Equal(expected, actual);
            }
        }
    }
}
