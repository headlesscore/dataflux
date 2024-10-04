using System.IO;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class NUnitTaskTest : ProcessExecutorTestFixtureBase
	{
		private NUnitTask task;

		[Fact]
		public void LoadWithSingleAssemblyNunitPath()
		{
			const string xml = @"<nunit>
	<path>d:\temp\nunit-console.exe</path>
	<assemblies>
		<assembly>foo.dll</assembly>
	</assemblies>
	<outputfile>c:\testfile.xml</outputfile>
	<timeout>50</timeout>
</nunit>";
			task = (NUnitTask) NetReflector.Read(xml);
			Assert.Equal(@"d:\temp\nunit-console.exe", task.NUnitPath);
			Assert.Equal(1, task.Assemblies.Length);
			Assert.Equal("foo.dll", task.Assemblies[0]);
			Assert.Equal(@"c:\testfile.xml", task.OutputFile);
			Assert.Equal(50, task.Timeout);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void LoadWithMultipleAssemblies()
		{
			const string xml = @"<nunit>
							 <path>d:\temp\nunit-console.exe</path>
				             <assemblies>
			                     <assembly>foo.dll</assembly>
								 <assembly>bar.dll</assembly>
							</assemblies>
						 </nunit>";

			task = (NUnitTask) NetReflector.Read(xml);
			AssertEqualArrays(new string[] {"foo.dll", "bar.dll"}, task.Assemblies);
		}

        [Fact]
        public void LoadWithExcludedCategories()
        {
            const string xml = @"<nunit>
							 <path>d:\temp\nunit-console.exe</path>
				             <assemblies>
			                     <assembly>foo.dll</assembly>
								 <assembly>bar.dll</assembly>
							</assemblies>
                            <excludedCategories>
				                <excludedCategory>Category1</excludedCategory>
				                <excludedCategory>Category 2</excludedCategory>				
				                <excludedCategory> </excludedCategory>				
			                </excludedCategories>
						 </nunit>";

            task = (NUnitTask) NetReflector.Read(xml);
            Assert.Equal(3, task.ExcludedCategories.Length);
        }

		[Fact]
		public void HandleNUnitTaskFailure()
		{
			CreateProcessExecutorMock(NUnitTask.DefaultPath);
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			IIntegrationResult result = IntegrationResult();
			result.ArtifactDirectory = Path.GetTempPath();

			task = new NUnitTask((ProcessExecutor) mockProcessExecutor.Object);
			task.Assemblies = new string[] {"foo.dll"};
			task.Run(result);

			Assert.Equal(1, result.TaskResults.Count);
		    Assert.True(result.TaskOutput, Is.Empty);
			Verify();
		}
	}
}
