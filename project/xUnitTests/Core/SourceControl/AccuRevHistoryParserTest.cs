using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	/// <remarks>
	/// This code is based on code\sourcecontrol\ClearCase.cs.
	/// </remarks>
	
	public class AccuRevHistoryParserTest 
	{
		AccuRevHistoryParser parser;

		// [SetUp]
		protected void Setup()
		{
			parser = new AccuRevHistoryParser();
		}

		/// <summary>
		/// Test that we still know how to parse the output of an AccuRev "accurev hist" command.
		/// </summary>
		/// <remarks>
		/// This tests either the Windows or the Unix version of the output, depending on which 
		/// type of system the test is being executed on.  I'd love to test both on all systems,
		/// but that would mean faking out ExecutionEnvironment and we're not ready for that yet.
		/// </remarks> 
		[Fact]
		public void CanParse()
		{
            AccuRevMother histData = AccuRevMother.GetInstance();
		    TextReader historyReader = histData.historyOutputReader;

            Modification[] mods = parser.Parse(historyReader, histData.oldestHistoryModification,
                                               histData.newestHistoryModification);
			Assert.NotNull(mods);
            Assert.True(true);
            Assert.Equal(histData.historyOutputModifications.Length, mods.Length);
		    for (int i = 0; i < histData.historyOutputModifications.Length; i++)
		    {
		        Assert.Equal(histData.historyOutputModifications[i].ChangeNumber, mods[i].ChangeNumber);
		        Assert.Equal(histData.historyOutputModifications[i].Comment, mods[i].Comment);
		        Assert.Equal(histData.historyOutputModifications[i].FileName, mods[i].FileName);
		        Assert.Equal(histData.historyOutputModifications[i].FolderName, mods[i].FolderName);
		        Assert.Equal(histData.historyOutputModifications[i].ModifiedTime, mods[i].ModifiedTime);
		        Assert.Equal(histData.historyOutputModifications[i].Type, mods[i].Type);
		        Assert.Equal(histData.historyOutputModifications[i].UserName, mods[i].UserName);
		    }
		}
	}
}
