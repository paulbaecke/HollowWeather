namespace HollowMan.Core.Test
{
    using HollowMan.Core.SensorData;
    using HollowMan.Core.Sensors.Drivers;
    using HollowMan.Core.Shared;
    using Moq;
    using System;
    using Xunit;

    public class BME280Tests
    {
        [Fact]
        public void CalculateTemperatureTest()
        {
            var cal1 = new byte[24];
            var cal2 = new byte[1];
            var cal3 = new byte[7];
            var digT1 = BitConverter.GetBytes((ushort)28154);
            var digT2 = BitConverter.GetBytes((short)26709);
            var digT3 = BitConverter.GetBytes((short)50);
            cal1[0] = digT1[0];
            cal1[1] = digT1[1];
            cal1[2] = digT2[0];
            cal1[3] = digT2[1];
            cal1[4] = digT3[0];
            cal1[5] = digT3[1];
            
            var calib = BME280.ExtractcalibrationData(cal1, cal2, cal3);
            var sensorResult = new Mock<SensorSample>("test");            
            var result = BME280.CalculateTemperature(sensorResult.Object, calib, 345408, out double tFine);
            Assert.Equal(32.03, result);
            Assert.Equal(163980.7041, tFine);
        }
    }
}
