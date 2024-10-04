using System;
using System.IO;
using System.Xml;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class ProcessTaskResultTest
	{
		private StringWriter writer;

		[Fact]
		public void CheckIfSuccessIfProcessResultSucceeded()
		{
			ProcessTaskResult result = new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult());
			Assert.True(result.CheckIfSuccess());
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void FailedIfProcessResultFailed()
		{
			ProcessTaskResult result = new ProcessTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult());
			Assert.False(result.CheckIfSuccess());
		}

		[Fact]
		public void FailedIfProcessResultTimedout()
		{
			ProcessTaskResult result = new ProcessTaskResult(ProcessResultFixture.CreateTimedOutResult());
			Assert.False(result.CheckIfSuccess());
		}

		[Fact]
		public void DataShouldBeStdOutIfNoStdErr()
		{
			ProcessResult processResult = new ProcessResult("stdout", null, 5, false);
			ProcessTaskResult result = new ProcessTaskResult(processResult);
			Assert.Equal("stdout", result.Data);
		}

		[Fact]
		public void DataShouldBeStdOutAndStdErrIfStdErrExists()
		{
			ProcessResult processResult = new ProcessResult("stdout", "error", 5, false);
			ProcessTaskResult result = new ProcessTaskResult(processResult);
			Assert.Equal(string.Format(System.Globalization.CultureInfo.CurrentCulture,"stdout{0}error", Environment.NewLine), result.Data);
		}

		[Fact]
		public void WriteProcessResultToXml()
		{
			Assert.Equal("<task><standardOutput>foo</standardOutput><standardError>bar</standardError></task>", 
				WriteToXml("foo", "bar", ProcessResult.SUCCESSFUL_EXIT_CODE, false));
		}

		[Fact]
		public void WriteFailedProcessResultToXml()
		{
			Assert.Equal(@"<task failed=""True""><standardOutput /><standardError>bar</standardError></task>", 
				WriteToXml(null, "bar", -3, false));
		}

		[Fact]
		public void WriteTimedOutProcessResultToXml()
		{
			Assert.Equal(@"<task failed=""True"" timedout=""True""><standardOutput /><standardError>bar</standardError></task>", 
				WriteToXml(null, "bar", ProcessResult.TIMED_OUT_EXIT_CODE, true));
		}

		private string WriteToXml(string output, string error, int errorCode, bool timedOut)
		{
			TaskResult(output, error, errorCode, timedOut).WriteTo(XmlWriter());
			return writer.ToString();
		}

		private ProcessTaskResult TaskResult(string output, string error, int errorCode, bool timedOut)
		{
			return new ProcessTaskResult(new ProcessResult(output, error, errorCode, timedOut));
		}

		private XmlWriter XmlWriter()
		{
			writer = new StringWriter();
			return new XmlTextWriter(writer);
		}
	}
}
