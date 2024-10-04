using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class FilteredSourceControlTest: CustomAssertion
	{
		private const string SourceControlXml =
			@"<sourcecontrol type=""filtered"">
				<sourceControlProvider type=""mocksourcecontrol"">
						<anOptionalProperty>foo</anOptionalProperty>
				</sourceControlProvider>
				<inclusionFilters>
					<pathFilter>
						<pattern>/sources/**/*.*</pattern>
					</pathFilter>
				</inclusionFilters>
                <exclusionFilters>
                    <pathFilter>
						<pattern>/sources/info/version.cs</pattern>
                    </pathFilter>
                </exclusionFilters>
              </sourcecontrol>";
		private FilteredSourceControl _filteredSourceControl;
		private Mock<ISourceControl> _mockSC;

		// [SetUp]
		public void SetUp()
		{
			_filteredSourceControl = new FilteredSourceControl();
			_mockSC = new Mock<ISourceControl>();
			_filteredSourceControl.SourceControlProvider = (ISourceControl)_mockSC.Object;
		}

		// [TearDown]
		public void TearDown()
		{
			_mockSC.Verify();			
		}

		[Fact]
		public void ValuePopulation()
		{
			//// EXECUTE
			NetReflector.Read(SourceControlXml, _filteredSourceControl);

			//// VERIFY
			Assert.True(_filteredSourceControl.SourceControlProvider != null);
            Assert.True(true);

            string optionalProp = ((SourceControlMock)_filteredSourceControl.SourceControlProvider).AnOptionalProperty;
			Assert.Equal(optionalProp, "foo", "Didn't find expected source control provider");

			Assert.Equal(_filteredSourceControl.InclusionFilters.Length, 1);

			string inclusionPattern = ((PathFilter)_filteredSourceControl.InclusionFilters[0]).Pattern;
			Assert.Equal(inclusionPattern, "/sources/**/*.*", "Didn't find expected inclusion path pattern");

			Assert.Equal(_filteredSourceControl.ExclusionFilters.Length, 1);

			string exclusionPattern = ((PathFilter)_filteredSourceControl.ExclusionFilters[0]).Pattern;
			Assert.Equal(exclusionPattern, "/sources/info/version.cs", "Didn't find expected exclusion path pattern");
		}

		[Fact]
		public void PassesThroughLabelSourceControl()
		{
			//// SETUP
			IntegrationResult result = new IntegrationResult();
			_mockSC.Setup(sc => sc.LabelSourceControl(result)).Verifiable();

			//// EXECUTE
			_filteredSourceControl.LabelSourceControl(result);
		}

		[Fact]
		public void PassesThroughGetSource()
		{
			//// SETUP
			IntegrationResult result = new IntegrationResult();
			_mockSC.Setup(sc => sc.GetSource(result)).Verifiable();

			//// EXECUTE
			_filteredSourceControl.GetSource(result);
		}

		[Fact]
		public void AppliesFiltersOnModifications()
		{
			//// SETUP
			IntegrationResult from = IntegrationResult(DateTime.Now);
			IntegrationResult to = IntegrationResult(DateTime.Now.AddDays(10));
			_mockSC.Setup(sc => sc.GetModifications(from, to)).Returns(Modifications).Verifiable();

			NetReflector.Read(SourceControlXml, _filteredSourceControl);
			_filteredSourceControl.SourceControlProvider = (ISourceControl)_mockSC.Object;

			//// EXECUTE
			Modification[] filteredResult = _filteredSourceControl.GetModifications(from, to);

			//// VERIFY
			Assert.Equal(1, filteredResult.Length);
		}

		private IntegrationResult IntegrationResult(DateTime dateTime1)
		{
			return IntegrationResultMother.CreateSuccessful(dateTime1);
		}

		public static readonly Modification[] Modifications = new Modification[]
				{
					ModificationMother.CreateModification("project.info", "/"),
					ModificationMother.CreateModification("test.csproj", "/sources"),
					ModificationMother.CreateModification("version.cs", "/sources/info")
				};
        public static readonly Modification[] ModificationsWithCVS = new Modification[]
                {
                    ModificationMother.CreateModification("x.cs", "/working/sources"),
                    ModificationMother.CreateModification("Entries", "/working/sources/CVS"),
                    ModificationMother.CreateModification("x.build", "/working/build"),
                    ModificationMother.CreateModification("x.dll", "/working/build/target/sources")                 
                };
                
        private const string SourceControlXmlWithCVS =
            @"<sourcecontrol type=""filtered"">
                <sourceControlProvider type=""mocksourcecontrol"">
                        <anOptionalProperty>foo</anOptionalProperty>
                </sourceControlProvider>
                <inclusionFilters>
                    <pathFilter>
                        <pattern>**/sources/**/*.*</pattern>
                    </pathFilter>
                    <pathFilter>
                        <pattern>**/build/**/*.*</pattern>
                    </pathFilter>
                </inclusionFilters>
                <exclusionFilters>
                    <pathFilter>
                        <pattern>**/CVS/**/*.*</pattern>
                    </pathFilter>
                    <pathFilter>
                        <pattern>**/target/**/*.*</pattern>
                    </pathFilter>
                </exclusionFilters>
              </sourcecontrol>";
              
        [Fact]
        public void AppliesInclusionExclusionOnModifications()
        {
            // Setup
            IntegrationResult from = IntegrationResult(DateTime.Now);
            IntegrationResult to = IntegrationResult(DateTime.Now.AddDays(10));
            _mockSC.Setup(sc => sc.GetModifications(from, to)).Returns(ModificationsWithCVS).Verifiable();
            
            NetReflector.Read(SourceControlXmlWithCVS, _filteredSourceControl);
            _filteredSourceControl.SourceControlProvider = (ISourceControl)_mockSC.Object;

            //// EXECUTE
            Modification[] filteredResult = _filteredSourceControl.GetModifications(from, to);

            //// VERIFY
            Assert.Equal(2, filteredResult.Length);     
            Assert.Equal("x.cs", filteredResult[0].FileName);
            Assert.Equal("x.build", filteredResult[1].FileName);
        }
    }
}
