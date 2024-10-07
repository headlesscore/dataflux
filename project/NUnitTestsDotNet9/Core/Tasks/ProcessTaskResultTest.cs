using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	[TestFixture]
	public class ProcessTaskResultTest
	{
		private StringWriter writer;

		[Test]
		public void CheckIfSuccessIfProcessResultSucceeded()
		{
			ProcessTaskResult result = new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult());
			ClassicAssert.IsTrue(result.CheckIfSuccess());
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void FailedIfProcessResultFailed()
		{
			ProcessTaskResult result = new ProcessTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult());
			ClassicAssert.IsFalse(result.CheckIfSuccess());
		}

		[Test]
		public void FailedIfProcessResultTimedout()
		{
			ProcessTaskResult result = new ProcessTaskResult(ProcessResultFixture.CreateTimedOutResult());
			ClassicAssert.IsFalse(result.CheckIfSuccess());
		}

		[Test]
		public void DataShouldBeStdOutIfNoStdErr()
		{
			ProcessResult processResult = new ProcessResult("stdout", null, 5, false);
			ProcessTaskResult result = new ProcessTaskResult(processResult);
			ClassicAssert.AreEqual("stdout", result.Data);
		}

		[Test]
		public void DataShouldBeStdOutAndStdErrIfStdErrExists()
		{
			ProcessResult processResult = new ProcessResult("stdout", "error", 5, false);
			ProcessTaskResult result = new ProcessTaskResult(processResult);
			ClassicAssert.AreEqual(string.Format(System.Globalization.CultureInfo.CurrentCulture,"stdout{0}error", Environment.NewLine), result.Data);
		}

		[Test]
		public void WriteProcessResultToXml()
		{
			ClassicAssert.AreEqual("<task><standardOutput>foo</standardOutput><standardError>bar</standardError></task>", 
				WriteToXml("foo", "bar", ProcessResult.SUCCESSFUL_EXIT_CODE, false));
		}

		[Test]
		public void WriteFailedProcessResultToXml()
		{
			ClassicAssert.AreEqual(@"<task failed=""True""><standardOutput /><standardError>bar</standardError></task>", 
				WriteToXml(null, "bar", -3, false));
		}

		[Test]
		public void WriteTimedOutProcessResultToXml()
		{
			ClassicAssert.AreEqual(@"<task failed=""True"" timedout=""True""><standardOutput /><standardError>bar</standardError></task>", 
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
