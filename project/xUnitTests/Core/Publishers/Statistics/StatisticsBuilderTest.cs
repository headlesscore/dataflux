using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Publishers.Statistics;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers.Statistics
{
    
    public class StatisticsBuilderTest : CustomAssertion
    {
        #region Setup/Teardown

        // [SetUp]
        public void SetUp()
        {
            builder = new StatisticsBuilder();
            result = IntegrationResultMother.CreateSuccessful();
        }

        #endregion

        private StatisticsBuilder builder;
        private IntegrationResult result;
        private string successfulBuildLog;
        private string failedBuildLog;
        private StatisticsResults results;

        // [OneTimeSetUp]
        public void LoadXML()
        {
            StreamReader reader = File.OpenText("buildlog.xml");
            successfulBuildLog = reader.ReadToEnd();
            reader.Close();
            reader = File.OpenText("failedbuildlog.xml");
            failedBuildLog = reader.ReadToEnd();
            reader.Close();
        }

        public void AssertHasStatistic(string name, object value, StatisticsResults results)
        {
            Assert.True(value ==
                            results.Find(delegate(StatisticResult obj) { return obj.StatName.Equals(name); }).Value,
                            $"Wrong statistic for {name}");
        }

        [Fact]
        public void ShouldCollectFxCopStatistics()
        {
            results = builder.ProcessBuildResults(successfulBuildLog);

            AssertHasStatistic("FxCop Warnings", 9, results);
            AssertHasStatistic("FxCop Errors", 202, results);
        }

        [Fact]
        public void ShouldGetFailureReasonForFailedBuildResult()
        {
            FirstMatch failureTypeStat = new FirstMatch("BuildErrorType", "//failure/builderror/type");
            FirstMatch failureMessageStat = new FirstMatch("BuildErrorMessage", "//failure/builderror/message");
            XmlDocument document = new XmlDocument();
            document.LoadXml(failedBuildLog);
            XPathNavigator navigator = document.CreateNavigator();

            string failureType = Convert.ToString(failureTypeStat.Apply(navigator).Value);
            string failureMessage = Convert.ToString(failureMessageStat.Apply(navigator).Value);

            Assert.True(failedBuildLog.IndexOf("builderror") > 0);
            Assert.Equal("NAnt.Core.BuildException", failureType);
            Assert.Equal(
                @"External Program Failed: c:\sf\ccnet\tools\ncover\NCover.Console.exe (return code was 1)",
                failureMessage);
        }

        [Fact]
        public void ShouldNotAddStatisticWithSameName()
        {
            int count = builder.Statistics.Count;
            Assert.True(count > 0);
            builder.Add(new Statistic("abc", "cdf"));
            Assert.Equal(count + 1, builder.Statistics.Count);
            builder.Add(new Statistic("abc", "cdf"));
            Assert.Equal(count + 1, builder.Statistics.Count);
        }

        [Fact]
        public void ShouldPopulateNUnitSummaryFromLog()
        {
            string xml =
                @"<task>
					<test-results total=""6"" failures=""1"" not-run=""2"" date=""2005-04-29"" time=""9:02 PM"">
						<test-suite />
					</test-results>
					<test-results total=""1"" failures=""1"" not-run=""1"" date=""2005-04-29"" time=""9:02 PM"">
						<test-suite />
					</test-results>
				</task>";

            result.AddTaskResult(xml);

            results = builder.ProcessBuildResults(result);

            AssertHasStatistic("TestCount", 7, results);
            AssertHasStatistic("TestFailures", 2, results);
            AssertHasStatistic("TestIgnored", 3, results);
        }

        [Fact]
        public void ShouldPopulateTimingsFromIntegrationResult()
        {
            result.StartTime = new DateTime(2005, 03, 12, 01, 13, 00);
            result.EndTime = new DateTime(2005, 03, 12, 01, 45, 00);
            result.ProjectName = "Foo";

            results = builder.ProcessBuildResults(result);

            AssertHasStatistic("StartTime", DateUtil.FormatDate(result.StartTime), results);
            AssertHasStatistic("Duration", new TimeSpan(0, 32, 0).ToString(), results);
            //AssertHasStatistic("ProjectName", "Foo", results);
        }

    }
}
