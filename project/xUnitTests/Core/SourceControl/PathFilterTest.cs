using Exortech.NetReflector;
using Xunit;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using System;
using Exortech.NetReflector;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
    
	public class PathFilterTest
	{
		[Fact]
		public void ExactFileNameMatch()
		{
			ExactFileNameTestSet.RunTests();
		}

		[Fact]
		public void AnyFileNameMatch()
		{
			AnyFileNameTestSet.RunTests();
		}

		[Fact]
		public void ExactFolderAnyNameMatch()
		{
			ExactFolderAnyNameTestSet.RunTests();
		}

		[Fact]
		public void AnyFolderExactNameMatch()
		{
			AnyFolderExactNameTestSet.RunTests();
		}

		[Fact]
		public void ExactSubfolderAnyNameMatch()
		{
			ExactSubfolderAnyNameTestSet.RunTests();
		}

		[Fact]
		public void AcceptAllMatch()
		{
			AcceptAllTestSet.RunTests();
		}

		[Fact]
		public void PartialNameMatch() 
		{
			PartialNameTestSet.RunTests();
		}

		[Fact]
		public void AnyFolderExactExtensionMatch()
		{
			AnyFolderExactExtensionTestSet.RunTests();
		}

		[Fact]
		public void PartialFolderAnyNameMatch() 
		{
			PartialFolderAnyNameTestSet.RunTests();
		}

		[Fact]
		public void AnyFolderPartialExtensionMatch()
		{
			AnyFolderPartialExtentsionTestSet.RunTests();
		}

		[Fact]
		public void PartialPathAnyNameMatch() 
		{
			PartialPathAnyNameTestSet.RunTests();
		}

		[Fact]
		public void ShouldNotAcceptModificationsWithNullFolder()
		{
			Modification modification = new Modification();
			PathFilter filter = new PathFilter();
			filter.Pattern = "*.*";
			Assert.False(filter.Accept(modification));
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldNotAcceptModificationsWithNullFilename()
		{
			Modification modification = new Modification();
			modification.FolderName = "c:\\";
			PathFilter filter = new PathFilter();
			filter.Pattern = "c:\\*.*";
			Assert.False(filter.Accept(modification));
		}
	    
	    [Fact]
	    public void DeeplyNestedFilters()
	    {
            Modification[] modlist = new Modification[]
                {
                    ModificationMother.CreateModification("x.cs", "/working/sources"),
                    ModificationMother.CreateModification("Entries", "/working/sources/CVS"),
                    ModificationMother.CreateModification("x.build", "/working/build"),
                    ModificationMother.CreateModification("x.dll", "/working/build/target/sources")                 
                };
	        PathFilter filter = new PathFilter();
	        filter.Pattern = "**/sources/**/*.*";
	        filter.Accept(modlist[0]);
        }

        [Fact]
        public void TwicePartialPathAnyNameMatch()
        {
            TwicePartialPathAnyNameTestSet.RunTests();
        }

        [Fact]
        public void CaseSensitivityTest()
        {
            Modification m = ModificationMother.CreateModification("x.xml", "/working/sources");
            PathFilter filter = new PathFilter();            
            filter.Pattern = "**/*.xml";
            Assert.True(filter.Accept(m));
            m.FileName = "test.Xml";
            Assert.False(filter.Accept(m));
            filter.CaseSensitive = false;
            Assert.True(filter.Accept(m));
        }
		private static Modification[] Modifications = 
			{
				ModificationMother.CreateModification("theName.dat",string.Empty),
				ModificationMother.CreateModification("TheName.dat",string.Empty),
				ModificationMother.CreateModification("theName.dat", "/theFolder"),
				ModificationMother.CreateModification("theFile.bin",string.Empty),
				ModificationMother.CreateModification("theFile.bin", "/theFolder"),
				ModificationMother.CreateModification("theName.dat", "/TheFolder"),
				ModificationMother.CreateModification("theName.dat", "/theFolder/theSubFolder"),
				ModificationMother.CreateModification("theName.dat", "/theFolder/theSubFolder/theSubSubFolder"),
				ModificationMother.CreateModification("theName.dat", "/theFolder/theSubFolder/theSubFolder"),
				ModificationMother.CreateModification("theName.dat", "/theFolder/theSubFolder/ThesubFolder"),
				ModificationMother.CreateModification("theName",string.Empty),
				ModificationMother.CreateModification("theName", "/theFolder/theSubFolder"),
				ModificationMother.CreateModification("theName.dav", "\\theFolder\\theSubFolder"),
				ModificationMother.CreateModification("theName.dav", "\\theFolder")
			};

		public static readonly string ExactFileNameFilterXml =
			@"<pathFilter>
                  <pattern>theName.dat</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper ExactFileNameTestSet =
			new PathFilterTestHelper(
			ExactFileNameFilterXml, 
			Modifications, 
			new bool[] {true, false, false, false, false, 
						   false, false, false, false, false, 
						   false, false, false, false});

		public static readonly string AnyFileNameFilterXml =
			@"<pathFilter>
			      <pattern>*.*</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper AnyFileNameTestSet =
			new PathFilterTestHelper(
			AnyFileNameFilterXml,
			Modifications,
			new bool[] {true, true, false, true, false, 
						   false, false, false, false, false, 
						   true, false, false, false});

		public static string ExactFolderAnyNameFilterXml =
			@"<pathFilter>
                  <pattern>/theFolder/*.*</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper ExactFolderAnyNameTestSet =
			new PathFilterTestHelper(
			ExactFolderAnyNameFilterXml,
			Modifications,
			new bool[] {false, false, true, false, true, 
						   false, false, false, false, false, 
						   false, false, false, true});

		public static string AnyFolderExactNameFilterXml =
			@"<pathFilter>
                  <pattern>**/theName.dat</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper AnyFolderExactNameTestSet =
			new PathFilterTestHelper(
			AnyFolderExactNameFilterXml,
			Modifications,
			new bool[] {true, false, true, false, false, 
						   true, true, true, true, true, 
						   false, false, false, false});

		public static string ExactSubfolderAnyNameFilterXml = 
			@"<pathFilter>
                  <pattern>**/theSubFolder/*.*</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper ExactSubfolderAnyNameTestSet =
			new PathFilterTestHelper(
			ExactSubfolderAnyNameFilterXml,
			Modifications,
			new bool[] {false, false, false, false, false, 
						   false, true, false, true, false, 
						   false, true, true, false});

		public static string AcceptAllFilterXml =
			@"<pathFilter>
                  <pattern>**/*.*</pattern>
              </pathFilter>"; 

		public static readonly PathFilterTestHelper AcceptAllTestSet =
			new PathFilterTestHelper(
			AcceptAllFilterXml,
			Modifications,
			new bool[] {true, true, true, true, true, 
						   true, true, true, true, true, 
						   true, true, true, true});

		public static string PartialNameFilterXml =
			@"<pathFilter>
                  <pattern>**/the*.dat</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper PartialNameTestSet =
			new PathFilterTestHelper(
			PartialNameFilterXml,
			Modifications,
			new bool[] {true, false, true, false, false, 
						   true, true, true, true, true, 
						   false, false, false, false});

		public static string AnyFolderExactExtensionFilterXml =
			@"<pathFilter>
                  <pattern>**/*.bin</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper AnyFolderExactExtensionTestSet =
			new PathFilterTestHelper(
			AnyFolderExactExtensionFilterXml,
			Modifications,
			new bool[] {false, false, false, true, true, 
						   false, false, false, false, false, 
						   false, false, false, false});

		public static string PartialFolderAnyNameFilterXml =
			@"<pathFilter>
                  <pattern>**/the*Folder/*.*</pattern>
              </pathFilter>";

		public static readonly PathFilterTestHelper PartialFolderAnyNameTestSet =
			new PathFilterTestHelper(
			PartialFolderAnyNameFilterXml,
			Modifications,
			new bool[] {false, false, true, false, true, 
						   false, true, true, true, false, 
						   false, true, true, true});

		public static string AnyFolderPartialExtensionFilterXml =
			@"<pathFilter>
                  <pattern>**/*.da*</pattern>
              </pathFilter>";

		public static PathFilterTestHelper AnyFolderPartialExtentsionTestSet =
			new PathFilterTestHelper(
			AnyFolderPartialExtensionFilterXml,
			Modifications,
			new bool[] {true, true, true, false, false,
						   true, true, true, true, true,
						   false, false, true, true});

		public static string PartialPathAnyNameFilterXml =
			@"<pathFilter>
                  <pattern>/theFolder/**/*.*</pattern>
              </pathFilter>";

		public static PathFilterTestHelper PartialPathAnyNameTestSet =
			new PathFilterTestHelper(
			PartialPathAnyNameFilterXml,
			Modifications,
			new bool[] {false, false, true, false, true,
						   false, true, true, true, true,
						   false, true, true, true});

        public static string TwicePartialPathAnyNameFilterXml =
    @"<pathFilter>
        <pattern>**/theSubFolder/**/*.*</pattern>
      </pathFilter>";

        public static PathFilterTestHelper TwicePartialPathAnyNameTestSet =
            new PathFilterTestHelper(
              TwicePartialPathAnyNameFilterXml,
              Modifications,
              new bool[] {false, false, false, false, false,
                            false, true , true , true , true ,
                            false, true , true , false});
	}

	public class PathFilterTestHelper 
	{
		private PathFilter filter;
		private Modification[] modifications;
		private bool[] expectedResults;

		public PathFilterTestHelper(string xmlFragment,
			Modification[] modifications,
			bool[] expectedResults) 
		{
			filter = new PathFilter();
			NetReflector.Read(xmlFragment, filter);

			this.modifications = modifications;
			this.expectedResults = expectedResults;
		}

		public void RunTests() 
		{
			int i = 0;

			foreach (Modification m in modifications) 
			{
				Assert.True(filter.Accept(m) == expectedResults[i], 
					DescribeExpectedResult(m, expectedResults[i]));
				i++;
			}
		}

		private string DescribeExpectedResult(
			Modification m,
			bool expectedResult) 
		{
			return string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0}/{1} should be {2}.",
				m.FolderName,
				m.FileName,
				expectedResult ? "accepted" : "rejected");
		}
	}
}
