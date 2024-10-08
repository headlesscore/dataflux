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
    
    public class FileXmlReaderTests
    {
        [Fact]
        public void GetSetAllProperties()
        {
            FileXmlReader logger = new FileXmlReader();
            string fileName = "LogFile.xml";
            logger.AuditFileLocation = fileName;
            Assert.Equal(fileName, logger.AuditFileLocation);
            Assert.True(true);
        }

        [Fact]
        public void ReadAllEvents()
        {
            FileXmlReader reader = new FileXmlReader();
            reader.AuditFileLocation = GenerateAuditFile();
            List<AuditRecord> records = reader.Read(0, 100);
            Assert.Equal(4, records.Count);
        }

        [Fact]
        public void ReadSomeEvents()
        {
            FileXmlReader reader = new FileXmlReader();
            reader.AuditFileLocation = GenerateAuditFile();
            List<AuditRecord> records = reader.Read(0, 2);
            Assert.Equal(2, records.Count);
        }

        [Fact]
        public void ReadAllFilteredEvents()
        {
            FileXmlReader reader = new FileXmlReader();
            reader.AuditFileLocation = GenerateAuditFile();
            List<AuditRecord> records = reader.Read(0, 100, AuditFilters.ByUser("User #1"));
            Assert.Equal(2, records.Count);
        }

        [Fact]
        public void ReadSomeFilteredEvents()
        {
            FileXmlReader reader = new FileXmlReader();
            reader.AuditFileLocation = GenerateAuditFile();
            List<AuditRecord> records = reader.Read(0, 1, AuditFilters.ByUser("User #1"));
            Assert.Single(records);
        }

        private string GenerateAuditFile()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "Log1.xml");
            if (File.Exists(logFile)) File.Delete(logFile);

            string auditData = string.Format(
                "<event><dateTime>{0:o}</dateTime><project>Project #1</project><user>User #1</user><type>{1}</type><outcome>{2}</outcome></event>" + Environment.NewLine +
                "<event><dateTime>{0:o}</dateTime><project>Project #2</project><user>User #1</user><type>{1}</type><outcome>{2}</outcome></event>" + Environment.NewLine +
                "<event><dateTime>{0:o}</dateTime><project>Project #1</project><user>User #2</user><type>{1}</type><outcome>{2}</outcome></event>" + Environment.NewLine +
                "<event><dateTime>{0:o}</dateTime><project>Project #2</project><user>User #2</user><type>{1}</type><outcome>{2}</outcome></event>" + Environment.NewLine,
                DateTime.Today,
                SecurityEvent.ForceBuild,
                SecurityRight.Allow);

            File.AppendAllText(logFile, auditData);

            return logFile;
        }
    }
}
