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
	
	public class StatisticsPublisherTest
	{
		private SystemPath testDir;

		// [SetUp]
		public void CreateFakeOutputDir()
		{
			testDir = SystemPath.UniqueTempPath();
		}

		// [TearDown]
		public void DeleteTempDirectory()
		{
			testDir.DeleteDirectory();
		}

		[Fact]
		public void CreatesStatisticsFileInArtifactDirectory()
		{
			IntegrationResult result1 = SimulateBuild(1);

			string statsFile = Path.Combine(result1.ArtifactDirectory, StatisticsPublisher.XmlFileName);
			Assert.True(File.Exists(statsFile));
            
            CountNodes(result1.ArtifactDirectory, "//statistics/integration", 1);

			IntegrationResult result2 = SimulateBuild(2);

			string statsFile2 = Path.Combine(result2.ArtifactDirectory, StatisticsPublisher.XmlFileName);
			Assert.True(File.Exists(statsFile2));

            CountNodes(result2.ArtifactDirectory, "//statistics/integration", 2);
		}

		[Fact]
		public void CreatesCsvFileInArtifactsDirectory()
		{
			IntegrationResult result1 = SimulateBuild(1);

			string statsFile = Path.Combine(result1.ArtifactDirectory, StatisticsPublisher.CsvFileName);
			Assert.True(File.Exists(statsFile));

            CountLines(statsFile, 2);

			IntegrationResult result2 = SimulateBuild(2);

			string statsFile2 = Path.Combine(result2.ArtifactDirectory, StatisticsPublisher.CsvFileName);
			Assert.True(File.Exists(statsFile2));

            CountLines(statsFile2, 3);
		}

		[Fact]
		public void LoadStatistics()
		{
			testDir.CreateDirectory().CreateTextFile(StatisticsPublisher.XmlFileName, "<statistics />");
			XmlDocument statisticsDoc =  new XmlDocument();
             statisticsDoc.LoadXml(StatisticsPublisher.LoadStatistics(testDir.ToString()));

			XPathNavigator navigator = statisticsDoc.CreateNavigator();
			XPathNodeIterator nodeIterator = navigator.Select("//timestamp/@day");
			
			nodeIterator.MoveNext();
			int day = Convert.ToInt32(nodeIterator.Current.Value);
			Assert.Equal(DateTime.Now.Day, day);

			nodeIterator = navigator.Select("//timestamp/@month");
			nodeIterator.MoveNext();
			string month = nodeIterator.Current.Value;
			Assert.Equal(DateTime.Now.ToString("MMM"), month);

			nodeIterator = navigator.Select("//timestamp/@year");
			nodeIterator.MoveNext();
			int year = Convert.ToInt32(nodeIterator.Current.Value);
			Assert.Equal(DateTime.Now.Year, year);
		}

		private IntegrationResult SimulateBuild(int buildLabel)
		{
			StatisticsPublisher publisher = new StatisticsPublisher();

			IntegrationResult result = IntegrationResultMother.CreateSuccessful(buildLabel.ToString());
			result.ArtifactDirectory = testDir.ToString();
			
			publisher.Run(result);
			
			return result;
		}

		private static void CountLines(string file, int expectedCount)
		{
			StreamReader text = File.OpenText(file);
			string s = text.ReadToEnd();
			string[] split = s.Split('\n');
			int count = 0;
			foreach(string line in split)
			{
				if (line.Length>0) count++;
			}
			Assert.Equal(expectedCount, count);
			text.Close();
		}

		private static void CountNodes(string artifactFolder, string xpath, int count)
		{
			XmlDocument doc = new XmlDocument();
			//doc.Load(statsFile2);
            doc.LoadXml( StatisticsPublisher.LoadStatistics(artifactFolder)); 
			XmlNodeList node = doc.SelectNodes(xpath);
			Assert.Equal(count, node.Count);
		}
	}
}
