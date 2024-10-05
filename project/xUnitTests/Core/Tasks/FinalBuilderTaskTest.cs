using System;
using System.IO;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class FinalBuilderTaskTest : ProcessExecutorTestFixtureBase
	{		
		private IIntegrationResult _result;
		private FinalBuilderTask _task;
		private Mock<IRegistry> _mockRegistry;
		private string fbExecutable;

		// [SetUp]
		protected void SetUp()
		{
			fbExecutable = Path.Combine(DefaultWorkingDirectory, "FBCmd.exe");

			_mockRegistry = new Mock<IRegistry>();
			CreateProcessExecutorMock(fbExecutable);
			_result = IntegrationResult();
			_result.Label = "1.0";
			_result.ArtifactDirectory = Path.GetTempPath();
			_task = new FinalBuilderTask((IRegistry) _mockRegistry.Object, (ProcessExecutor) mockProcessExecutor.Object);
		}

		// [TearDown]
		protected void TearDown()
		{
			Verify();
			_mockRegistry.Verify();
		}

		[Fact]
		public void BuildCommandLine()
		{
			string expectedArgs = @"/B /S /Vvar1=value1;var2=""value 2"" /P" + StringUtil.AutoDoubleQuoteString(Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5"));
			ExpectToExecuteArguments(expectedArgs);

			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder5.exe")).Verifiable();

			_task.FBVariables = new FBVariable[2];
			_task.FBVariables[0] = new FBVariable("var1", "value1");
			_task.FBVariables[1] = new FBVariable("var2", "value 2");
			_task.ProjectFile = Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5");
			_task.ShowBanner = false;
			_task.DontWriteToLog = true;
			_task.Timeout = 600;
			_task.Run(_result);

			Assert.Equal(1, _result.TaskResults.Count);
			Assert.Equal(IntegrationStatus.Success, _result.Status);
			Assert.Equal(ProcessResultOutput, _result.TaskOutput);
            Assert.True(true);
            Assert.True(true);
        }


        [Fact]
        public void BuildCommandLineOn64BitOs()
        {
            string expectedArgs = @"/B /S /Vvar1=value1;var2=""value 2"" /P" + StringUtil.AutoDoubleQuoteString(Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5"));
            ExpectToExecuteArguments(expectedArgs);

            _mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(() => null).Verifiable();
            _mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\Wow6432Node\VSoft\FinalBuilder\5.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder5.exe")).Verifiable();

            _task.FBVariables = new FBVariable[2];
            _task.FBVariables[0] = new FBVariable("var1", "value1");
            _task.FBVariables[1] = new FBVariable("var2", "value 2");
            _task.ProjectFile = Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5");
            _task.ShowBanner = false;
            _task.DontWriteToLog = true;
            _task.Timeout = 600;
            _task.Run(_result);

            Assert.Equal(1, _result.TaskResults.Count);
            Assert.Equal(IntegrationStatus.Success, _result.Status);
            Assert.Equal(ProcessResultOutput, _result.TaskOutput);
        }


		[Fact]
		public void DoubleQuoteSpacesinPaths()
		{
			string expectedArgs = string.Concat("/P", StringUtil.AutoDoubleQuoteString(Path.Combine(DefaultWorkingDirectoryWithSpaces, "TestProject.fbz5")));
			ExpectToExecuteArguments(expectedArgs, DefaultWorkingDirectoryWithSpaces);

			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder5.exe")).Verifiable();
							
			_task.ShowBanner = true;
			_task.ProjectFile = Path.Combine(DefaultWorkingDirectoryWithSpaces, "TestProject.fbz5");
			_task.Timeout = 600;
			_task.Run(_result);			

			Assert.Equal(1, _result.TaskResults.Count);
			Assert.Equal(IntegrationStatus.Success, _result.Status);
			Assert.Equal(ProcessResultOutput, _result.TaskOutput);		
		}

		[Fact]
		public void PopulateFromCompleteConfiguration()
		{
			const string xmlConfig = @"<FinalBuilder>
				<ProjectFile>C:\Dummy\Project.fbz3</ProjectFile>
 				<ShowBanner>false</ShowBanner>
 				<FBVariables>
 					<FBVariable name=""MyVariable"" value=""SomeValue"" />
 				</FBVariables>
				<FBVersion>3</FBVersion>
 				<FBCMDPath>C:\Program Files\MyFinalBuilderPath\FBCMD.EXE</FBCMDPath>
 				<DontWriteToLog>true</DontWriteToLog>
 				<Timeout>100</Timeout> 
				</FinalBuilder>";
			
			NetReflector.Read(xmlConfig, _task);
			Assert.Equal(@"C:\Dummy\Project.fbz3", _task.ProjectFile);
			Assert.Equal(false, _task.ShowBanner);
			Assert.Equal(1, _task.FBVariables.Length);
			Assert.Equal("MyVariable", _task.FBVariables[0].Name);
			Assert.Equal("SomeValue", _task.FBVariables[0].Value);
			Assert.Equal(3, _task.FBVersion);
			Assert.Equal(@"C:\Program Files\MyFinalBuilderPath\FBCMD.EXE", _task.FBCMDPath);
			Assert.Equal(100, _task.Timeout);		
		}

		[Fact]
		public void PopulateFromMinimalConfiguration()
		{	
			const string xmlConfig = @"<FinalBuilder>
								<ProjectFile>C:\Dummy\Project.fbz5</ProjectFile>
								</FinalBuilder>";

			// Get the FB5 path from the FB registry not the XML configuration.
			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder5.exe")).Verifiable();
					
			NetReflector.Read(xmlConfig, _task);

			Assert.Equal(@"C:\Dummy\Project.fbz5", _task.ProjectFile);
			Assert.Equal(5, _task.GetFBVersion());
			Assert.Equal(fbExecutable, _task.GetFBPath());
		}

		[Fact]
		public void AutodetectFB5Path()
		{
			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder5.exe")).Verifiable();
			_task.ProjectFile = @"C:\Dummy\Project.fbz5";
			Assert.Equal(5, _task.GetFBVersion());
            Assert.Equal(fbExecutable, _task.GetFBPath());
		}

		[Fact]
		public void AutodetectFB4Path()
		{
			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\4.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder4.exe")).Verifiable();
			_task.ProjectFile = @"C:\Dummy\Project.fbz4";
			Assert.Equal(4, _task.GetFBVersion());
            Assert.Equal(fbExecutable, _task.GetFBPath());
		}

		[Fact]
		public void AutodetectFB3Path()
		{
			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\3.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder4.exe")).Verifiable();
			_task.ProjectFile = @"C:\Dummy\Project.fbz3";
			Assert.Equal(3, _task.GetFBVersion());
            Assert.Equal(Path.Combine(DefaultWorkingDirectory, "FB3Cmd.exe"), _task.GetFBPath());
		}

        [Fact]
        public void InvalidProjectFileName()
        {
            NetReflector.Read(@"<FinalBuilder>
				<ProjectFile>C:\Dummy\Project.txt</ProjectFile>
			</FinalBuilder>", _task);

            Assert.Equal("Finalbuilder version could not be autodetected from project file name.",
                Assert.Throws<BuilderException>(() => { _task.Run(_result); }).Message);
            mockProcessExecutor.VerifyNoOtherCalls();
           _mockRegistry.VerifyNoOtherCalls();
        }

        [Fact]
        public void FinalBuilderIsNotInstalled()
        {
            _mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(() => null).Verifiable();
            _mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\Wow6432Node\VSoft\FinalBuilder\5.0", "Location")).Returns(() => null).Verifiable();

            _task.ProjectFile = @"C:\Dummy\Project.fbz5";

            Assert.Equal("Path to Finalbuilder 5 command line executable could not be found.",
                Assert.Throws<BuilderException>(()=> { _task.Run(_result); }).Message);

            mockProcessExecutor.VerifyNoOtherCalls();
        }

		[Fact]
		public void RequiredPropertiesNotProvided()
		{
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(@"<FinalBuilder />", _task); });
		}

        [Fact]
        public void TemporaryLogFile()
        {
			string expectedArgs = @"/B /TL /P" + StringUtil.AutoDoubleQuoteString(Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5")); 
            ExpectToExecuteArguments(expectedArgs);

			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder5.exe")).Verifiable();

			_task.ProjectFile = Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5");
            _task.UseTemporaryLogFile = true;
            _task.Timeout = 600;
            _task.Run(_result);

            Assert.Equal(1, _result.TaskResults.Count);
            Assert.Equal(IntegrationStatus.Success, _result.Status);
            Assert.Equal(ProcessResultOutput, _result.TaskOutput);
        }

        [Fact]
        public void TemporaryLogFileOverridesDontLogToOutput()
        {
			string expectedArgs = @"/B /TL /P" + StringUtil.AutoDoubleQuoteString(Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5"));
            ExpectToExecuteArguments(expectedArgs);

			_mockRegistry.Setup(registry => registry.GetLocalMachineSubKeyValue(@"SOFTWARE\VSoft\FinalBuilder\5.0", "Location")).Returns(Path.Combine(DefaultWorkingDirectory, "FinalBuilder5.exe")).Verifiable();

			_task.ProjectFile = Path.Combine(DefaultWorkingDirectory, "TestProject.fbz5");
            _task.UseTemporaryLogFile = true;
            _task.DontWriteToLog = true;
            _task.Timeout = 600;
            _task.Run(_result);

            Assert.Equal(1, _result.TaskResults.Count);
            Assert.Equal(IntegrationStatus.Success, _result.Status);
            Assert.Equal(ProcessResultOutput, _result.TaskOutput);
        }

	}
}
