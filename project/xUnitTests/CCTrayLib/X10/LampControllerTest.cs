using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.X10;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using System;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.X10
{
    public class TestFixture : IDisposable
    {
        internal LampController lampController;
        internal Mock<IX10LowLevelDriver> x10LowLevelDriverMock;
        internal const int GREEN_LAMP_DEVICE_CODE = 1;
        internal const int RED_LAMP_DEVICE_CODE = 2;
        internal const int YELLOW_LAMP_DEVICE_CODE = 3;

        public TestFixture() {
            x10LowLevelDriverMock = new Mock<IX10LowLevelDriver>(MockBehavior.Strict);

            X10Configuration configuration = new X10Configuration();
            configuration.SuccessUnitCode = GREEN_LAMP_DEVICE_CODE;
            configuration.BuildingUnitCode = YELLOW_LAMP_DEVICE_CODE;
            configuration.FailureUnitCode = RED_LAMP_DEVICE_CODE;
            IX10LowLevelDriver x10LowLevelDriver = x10LowLevelDriverMock.Object as IX10LowLevelDriver;
            lampController = new LampController(configuration, x10LowLevelDriver);
        }
        public void Dispose(){}
    }
    
    public class LampControllerTest : IClassFixture<TestFixture>, IDisposable
	{
        private readonly TestFixture _fixture;
        public LampControllerTest(TestFixture fixture) => _fixture = fixture;

        public void Dispose()
		{
			_fixture.x10LowLevelDriverMock.Verify();
		}
		
		[Fact]
		public void ShouldSendGreenControlCodeWhenTurningGreenLampOn()
		{
            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.GREEN_LAMP_DEVICE_CODE, Function.On, 0)).Verifiable();
            _fixture.lampController.GreenLightOn = true;
		}
		
		[Fact]
		public void ShouldSendRedControlCodeWhenTurningRedLampOn()
		{
            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.RED_LAMP_DEVICE_CODE, Function.On, 0)).Verifiable();
            _fixture.lampController.RedLightOn = true;
		}

		[Fact]
		public void ShouldSendGreenControlCodeWhenTurningGreenLampOff()
		{
            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.GREEN_LAMP_DEVICE_CODE, Function.Off, 0)).Verifiable();
            _fixture.lampController.GreenLightOn = false;
		}

		[Fact]
		public void ShouldSendRedControlCodeWhenTurningRedLampOff()
		{
            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.RED_LAMP_DEVICE_CODE, Function.Off, 0)).Verifiable();
            _fixture.lampController.RedLightOn = false;
		}
		
		[Fact]
		public void OnceTheLampHasBeenTurnedOnTurningItOnAgainDoesNotSendTheCommandAgain()
		{
            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.RED_LAMP_DEVICE_CODE, Function.Off, 0)).Verifiable();
            _fixture.lampController.RedLightOn = false;
            _fixture.lampController.RedLightOn = false;
            _fixture.lampController.RedLightOn = false;
            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.RED_LAMP_DEVICE_CODE, Function.On, 0)).Verifiable();
            _fixture.lampController.RedLightOn = true;
            _fixture.lampController.RedLightOn = true;
            _fixture.lampController.RedLightOn = true;

            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.GREEN_LAMP_DEVICE_CODE, Function.On, 0)).Verifiable();
            _fixture.lampController.GreenLightOn = true;
            _fixture.lampController.GreenLightOn = true;
            _fixture.lampController.GreenLightOn = true;
            _fixture.x10LowLevelDriverMock.Setup(driver => driver.ControlDevice(TestFixture.GREEN_LAMP_DEVICE_CODE, Function.Off, 0)).Verifiable();
            _fixture.lampController.GreenLightOn = false;
            _fixture.lampController.GreenLightOn = false;
            _fixture.lampController.GreenLightOn = false;

		}

	}
}
