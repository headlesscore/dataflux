using System;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class BuildTransitionSoundPlayerTest : IClassFixture<BuildTransitionSoundPlayerTest.TestFixture>, IDisposable
	{
        public class TestFixture : IDisposable
        {
            internal readonly StubProjectMonitor stubProjectMonitor;
            internal readonly Mock<IAudioPlayer> mockAudioPlayer;
            public TestFixture() {
                stubProjectMonitor = new StubProjectMonitor("project");

                mockAudioPlayer = new Mock<IAudioPlayer>(MockBehavior.Strict);
            }
            public void Dispose() { }
        }
        private readonly BuildTransitionSoundPlayerTest.TestFixture _fixture;
        public BuildTransitionSoundPlayerTest(BuildTransitionSoundPlayerTest.TestFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
		public void PlaysTheCorrectSoundFileWhenBuildTransitionsOccur()
		{
			AudioFiles files = new AudioFiles();
			files.StillFailingBuildSound = "anotherFailed.wav";
			files.StillSuccessfulBuildSound = "anotherSuccess.wav";
			files.BrokenBuildSound = "broken.wav";
			files.FixedBuildSound = "fixed.wav";

			new BuildTransitionSoundPlayer(
                _fixture.stubProjectMonitor, 
				(IAudioPlayer) _fixture.mockAudioPlayer.Object,
				files);

            _fixture.mockAudioPlayer.Setup(player => player.Play(files.BrokenBuildSound)).Verifiable();
            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.Broken));

			_fixture.mockAudioPlayer.Setup(player => player.Play(files.FixedBuildSound)).Verifiable();
            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.Fixed));

            _fixture.mockAudioPlayer.Setup(player => player.Play(files.StillFailingBuildSound)).Verifiable();
            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.StillFailing));

			_fixture.mockAudioPlayer.Setup(player => player.Play(files.StillSuccessfulBuildSound)).Verifiable();
            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.StillSuccessful));

			_fixture.mockAudioPlayer.Verify();
		}

		[Fact]
		public void WhenATransitionIsNullOrEmptyStringNoAudioIsPlayed()
		{
			AudioFiles files = new AudioFiles();
			files.StillSuccessfulBuildSound =string.Empty;
			files.StillFailingBuildSound = null;

			new BuildTransitionSoundPlayer(
				_fixture.stubProjectMonitor, 
				(IAudioPlayer)_fixture.mockAudioPlayer.Object,
				files);

            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.StillSuccessful));

            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.StillFailing));

            _fixture.mockAudioPlayer.VerifyNoOtherCalls();
		}

		[Fact]
		public void WhenNullIsPassedForTheConfigurationNoSoundsPlay()
		{
			new BuildTransitionSoundPlayer(
				_fixture.stubProjectMonitor, 
				(IAudioPlayer) _fixture.mockAudioPlayer.Object,
				null);

            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.StillSuccessful));

            _fixture.stubProjectMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(_fixture.stubProjectMonitor, BuildTransition.StillFailing));

            _fixture.mockAudioPlayer.VerifyNoOtherCalls();
		}

        public void Dispose() { }
    }
}
