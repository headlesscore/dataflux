using System;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class NullSourceControlTest
	{
		private NullSourceControl sourceControl;

		// [SetUp]
		public void Setup()
		{
			sourceControl = new NullSourceControl();
		}

		[Fact]
		public void ShouldReturnEmptyListOfModifications()
		{
			Assert.Equal(0, sourceControl.GetModifications(IntegrationResultMother.CreateSuccessful(DateTime.MinValue), IntegrationResultMother.CreateSuccessful(DateTime.MaxValue)).Length);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldReturnSilentlyForOtherOperations()
		{
			sourceControl.GetSource(null);
			sourceControl.Initialize(null);
			sourceControl.Purge(null);
			sourceControl.LabelSourceControl(null);
		}

        [Fact]
        public void ShouldFailGetModsWhenFailModsIsTrue()
        {
            sourceControl.FailGetModifications = true;
            Assert.True(delegate { sourceControl.GetModifications(null, null); },
                        Throws.TypeOf<Exception>().With.Message.EqualTo("Failing GetModifications"));
        }

        [Fact]
        public void ShouldFailGetSourceWhenFailGetSourceIsTrue()
        {
            sourceControl.FailGetSource = true;
            Assert.True(delegate { sourceControl.GetSource(null); },
                        Throws.TypeOf<Exception>().With.Message.EqualTo("Failing getting the source"));
        }

        [Fact]
        public void ShouldFailLabelSourceWhenFailLabelSourceIsTrue()
        {
            sourceControl.FailLabelSourceControl = true;
            Assert.True(delegate { sourceControl.LabelSourceControl(null); },
                        Throws.TypeOf<Exception>().With.Message.EqualTo("Failing label source control"));
        }

        [Fact]
        public void ShouldReturnNonEmptyListOfModificationsWhenAlwaysModifiedIsTrue()
        {
            sourceControl.AlwaysModified = true;
            Assert.NotEmpty(sourceControl.GetModifications(IntegrationResultMother.CreateSuccessful(DateTime.MinValue), IntegrationResultMother.CreateSuccessful(DateTime.MaxValue)));
        }

    }
}
