using System;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.BitKeeper;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Bitkeeper
{
	
	public class BitKeeperTest : ProcessExecutorTestFixtureBase
	{
		private BitKeeper bitkeeper;
		private Mock<IHistoryParser> mockHistoryParser;
		private DateTime from;
		private DateTime to;

		// [SetUp]
		protected void SetUp()
		{
			mockHistoryParser = new Mock<IHistoryParser>();
			CreateProcessExecutorMock(BitKeeper.DefaultExecutable);
			ProcessExecutor executor = (ProcessExecutor) mockProcessExecutor.Object;
			IHistoryParser parser = (IHistoryParser) mockHistoryParser.Object;
			bitkeeper = new BitKeeper(parser, executor);
			from = new DateTime(2001, 1, 21, 20, 0, 0);
			to = from.AddDays(1);
		}

		// [TearDown]
		protected void VerifyAll()
		{
			base.Verify();
			mockHistoryParser.Verify();
		}

		[Fact]
		public void VerifyGetModificationsCommandNonVerbose()
		{
			ExpectToExecuteArguments("changes -R");
			bitkeeper.GetModifications(IntegrationResult(from), IntegrationResult(to));
			VerifyAll();
		}

		[Fact]
		public void VerifyGetModificationsCommandVerbose()
		{
			ExpectToExecuteArguments("changes -R -v");
			bitkeeper.FileHistory = true;
			bitkeeper.GetModifications(IntegrationResult(from), IntegrationResult(to));
			VerifyAll();
		}

		[Fact]
		public void VerifyGetSourceCommand()
		{
			ExpectToExecuteArguments("pull");
			ExpectToExecuteArguments("push");
			bitkeeper.AutoGetSource = true;
			bitkeeper.GetSource(IntegrationResult());
			VerifyAll();
		}

		[Fact]
		public void VerifyGetSourceAndCloneCommand()
		{
			ExpectToExecuteArguments("pull");
			ExpectToExecuteArguments("push");
			ExpectToExecuteArguments("clone . /path/to/child");
			bitkeeper.AutoGetSource = true;
			bitkeeper.CloneTo = "/path/to/child";
			bitkeeper.GetSource(IntegrationResult());
			VerifyAll();
		}

		[Fact]
		public void VerifyLabelCommand()
		{
			ExpectToExecuteArguments("tag foo");
			ExpectToExecuteArguments("push");
			bitkeeper.TagOnSuccess = true;
			IIntegrationResult result = IntegrationResult();
			result.Label = "foo";
			bitkeeper.LabelSourceControl(result);
			VerifyAll();
		}
	}
}
