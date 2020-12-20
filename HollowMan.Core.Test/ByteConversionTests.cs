namespace HollowMan.Core.Test
{
    using HollowMan.Core.Shared;
    using Xunit;

    public class ByteConversionTests
    {
        /* Sample data 
        Cal 1/2/3
        [141, 110, 5, 103, 50, 0, 228, 145, 17, 214, 208, 11, 26, 48, 90, 255, 249, 255, 12, 48, 32, 209, 136, 19]
        [75]
        [70, 1, 0, 26, 35, 3, 30]
        T1: 28301
        T2: 26373
        H1: 75
        H4: 26
        raw
        [61, 85, 128, 130, 140, 0, 132, 12]
        sense
        251224
        534720
        33804
        131839
        19
        Temperature :  25.75 C
        Pressure :  1005.28379939 hPa
        Humidity :  34.5872020516 %

        */

        private readonly byte[] CalibrationBytes1 = new byte[] { 141, 110, 5, 103, 50, 0, 228, 145, 17, 214, 208, 11, 26, 48, 90, 255, 249, 255, 12, 48, 32, 209, 136, 19 };
        private readonly byte[] CalibrationBytes2 = new byte[] { 75 };
        private readonly byte[] CalibrationBytes3 = new byte[] { 70, 1, 0, 26, 35, 3, 30 };

        [Fact]
        public void ByteArrayToUShortTest()
        {
            var value = ByteOperations.GetUShort(CalibrationBytes1, 0);
            Assert.Equal(28301, value);
        }

        [Fact]
        public void ByteArrayToShortTest()
        {
            var value = ByteOperations.GetShort(CalibrationBytes1, 2);
            Assert.Equal(26373, value);
        }

        [Fact]
        public void ByteArrayToCharTest()
        {
            var value = ByteOperations.GetChar(CalibrationBytes3, 3);
            Assert.Equal(26, value);
        }

        [Fact]
        public void ByteArrayToUCharTest()
        {
            var value = ByteOperations.GetChar(CalibrationBytes2, 0);
            Assert.Equal(75, value);
        }
    }
}
