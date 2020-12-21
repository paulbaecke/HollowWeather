// <copyright file="BME280.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Collections.Generic;
    using System.Device.I2c;
    using System.Text;
    using System.Threading;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;
    using HollowMan.Core.Shared;
    using Iot.Device.Common;
    using UnitsNet;

    /// <summary>
    /// My original implementation of the BME280 driver.
    /// </summary>
    public class BME280 : Sensor
    {
        private const byte REGDATA = 0xF7;
        private const byte REGCONTROL = 0xF4;
        private const byte REGCONFIG = 0xF5;
        private const byte REGCONTROLHUM = 0xF2;
        private const byte REGHUMMSB = 0xFD;
        private const byte REGHUMLSB = 0xFE;
        private const byte OVERSAMPLETEMP = 2;
        private const byte OVERSAMPLEPRES = 2;
        private const byte MODE = 1;
        private const byte OVERSAMPLEHUM = 2;
        private const byte DEVICEID = 0x77;
        private const byte REGID = 0xD0;

        // This is used to correct the self-heating of the sensor
        // set using an external thermometer. Expected to be in the range
        // [-0.75, -5.21]
        private readonly double calibrationOffset;
        private readonly int altitudeInMeters;

        /// <summary>
        /// Initializes a new instance of the <see cref="BME280"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        /// <param name="altitudeInMeters">Altitude of location in meters. Used for corrections and calculating metrics.</param>
        /// <param name="calibrationOffset">The overheating correction to use.</param>
        public BME280(SensorController controller, double calibrationOffset, int altitudeInMeters)
            : this(controller, calibrationOffset, altitudeInMeters, DEVICEID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BME280"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        /// <param name="calibrationOffset">The overheating correction to use.</param>
        /// <param name="altitudeInMeters">Altitude of location in meters. Used for corrections and calculating metrics.</param>
        /// <param name="id">Override for I2c address.</param>
        public BME280(SensorController controller, double calibrationOffset, int altitudeInMeters, byte id)
        {
            this.Controller = controller;
            this.DeviceId = id;
            this.SensorName = "BME280_HMDriver";
            this.calibrationOffset = calibrationOffset;
            this.altitudeInMeters = altitudeInMeters;
        }

        /// <summary>
        /// Represents sensor digits.
        /// </summary>
        public enum SensorDigit
        {
            /// <summary>
            /// T1.
            /// </summary>
            DigitT1,

            /// <summary>
            /// T2.
            /// </summary>
            DigitT2,

            /// <summary>
            /// T3.
            /// </summary>
            DigitT3,

            /// <summary>
            /// P1.
            /// </summary>
            DigitP1,

            /// <summary>
            /// P2.
            /// </summary>
            DigitP2,

            /// <summary>
            /// P3.
            /// </summary>
            DigitP3,

            /// <summary>
            /// P4.
            /// </summary>
            DigitP4,

            /// <summary>
            /// P5.
            /// </summary>
            DigitP5,

            /// <summary>
            /// P6.
            /// </summary>
            DigitP6,

            /// <summary>
            /// P7.
            /// </summary>
            DigitP7,

            /// <summary>
            /// P8.
            /// </summary>
            DigitP8,

            /// <summary>
            /// P9.
            /// </summary>
            DigitP9,

            /// <summary>
            /// H1.
            /// </summary>
            DigitH1,

            /// <summary>
            /// H2.
            /// </summary>
            DigitH2,

            /// <summary>
            /// H3.
            /// </summary>
            DigitH3,

            /// <summary>
            /// H4.
            /// </summary>
            DigitH4,

            /// <summary>
            /// H5.
            /// </summary>
            DigitH5,

            /// <summary>
            /// H6.
            /// </summary>
            DigitH6,
        }

        /// <summary>
        /// Exctract data from raw byte response.
        /// </summary>
        /// <param name="cal1">First word.</param>
        /// <param name="cal2">Second word.</param>
        /// <param name="cal3">Third word.</param>
        /// <returns>Sensor digit dictionary.</returns>
        public static Dictionary<SensorDigit, int> ExtractSensorData(byte[] cal1, byte[] cal2, byte[] cal3)
        {
            var sensorData = new Dictionary<SensorDigit, int>(18)
            {
                { SensorDigit.DigitT1, ByteOperations.GetUShort(cal1, 0) },
                { SensorDigit.DigitT2, ByteOperations.GetShort(cal1, 2) },
                { SensorDigit.DigitT3, ByteOperations.GetShort(cal1, 4) },
                { SensorDigit.DigitP1, ByteOperations.GetUShort(cal1, 6) },
                { SensorDigit.DigitP2, ByteOperations.GetShort(cal1, 8) },
                { SensorDigit.DigitP3, ByteOperations.GetShort(cal1, 10) },
                { SensorDigit.DigitP4, ByteOperations.GetShort(cal1, 12) },
                { SensorDigit.DigitP5, ByteOperations.GetShort(cal1, 14) },
                { SensorDigit.DigitP6, ByteOperations.GetShort(cal1, 16) },
                { SensorDigit.DigitP7, ByteOperations.GetShort(cal1, 18) },
                { SensorDigit.DigitP8, ByteOperations.GetShort(cal1, 20) },
                { SensorDigit.DigitP9, ByteOperations.GetShort(cal1, 22) },
                { SensorDigit.DigitH1, ByteOperations.GetUChar(cal2, 0) },
                { SensorDigit.DigitH2, ByteOperations.GetUShort(cal3, 0) },
                { SensorDigit.DigitH3, ByteOperations.GetUChar(cal3, 2) },
                { SensorDigit.DigitH4, CalculateH4Offset(cal3, 3, 4) },
                { SensorDigit.DigitH5, CalculateH5Offset(cal3, 4, 5) },
                { SensorDigit.DigitH6, ByteOperations.GetChar(cal3, 6) },
            };

            return sensorData;
        }

        /// <summary>
        /// Get the the temperature from the digits extracted.
        /// </summary>
        /// <param name="digitT1">T1.</param>
        /// <param name="digitT3">T3.</param>
        /// <param name="rawTemperature">Uncalibrated temperatrue.</param>
        /// <returns>Temperature component.</returns>
        public static int GetTempPart2(int digitT1, int digitT3, int rawTemperature)
        {
            return (((((rawTemperature >> 4) - digitT1) * ((rawTemperature >> 4) - digitT1)) >> 12) * digitT3) >> 14;
        }

        /// <summary>
        /// Get the the temperature from the digits extracted.
        /// </summary>
        /// <param name="digitT1">T1.</param>
        /// <param name="digitT2">T2.</param>
        /// <param name="rawTemperature">Uncalibrated temperatrue.</param>
        /// <returns>Temperature component.</returns>
        public static int GetTempPart1(int digitT1, int digitT2, int rawTemperature)
        {
            // Adjust temperature based on calibration
            return (((rawTemperature >> 3) - (digitT1 << 1)) * digitT2) >> 11;
        }

        /// <summary>
        /// Calculate H4 offset.
        /// </summary>
        /// <param name="bytes">Raw bytes.</param>
        /// <param name="offset1">First offset.</param>
        /// <param name="offset2">Second offset.</param>
        /// <returns>H4 correction.</returns>
        public static int CalculateH4Offset(byte[] bytes, int offset1, int offset2)
        {
            var cal3_3 = ByteOperations.GetChar(bytes, offset1);
            var cal3_4 = ByteOperations.GetChar(bytes, offset2);
            return ((cal3_3 << 24) >> 20) | cal3_4;
        }

        /// <summary>
        /// Calculate H5 offset.
        /// </summary>
        /// <param name="bytes">Raw bytes.</param>
        /// <param name="offset1">First offset.</param>
        /// <param name="offset2">Second offset.</param>
        /// <returns>H5 correction.</returns>
        public static int CalculateH5Offset(byte[] bytes, int offset1, int offset2)
        {
            var cal3_4 = ByteOperations.GetChar(bytes, offset1);
            var cal3_5 = ByteOperations.GetChar(bytes, offset2);
            return ((cal3_5 << 24) >> 20) | cal3_4 >> 4 & 0x0F;
        }

        /// <summary>
        /// Calculate the wait time for readings. This helps with accuracy.
        /// See Bosch BME280 spec.
        /// </summary>
        /// <returns>Calculated wait time.</returns>
        public static double GetWaitTime()
        {
            return 1.25 + (2.3 * OVERSAMPLETEMP) + ((2.3 * OVERSAMPLEPRES) + 0.575) + ((2.3 * OVERSAMPLEHUM) + 0.575);
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();
            this.Device = this.Controller.GetI2CConnection(this.DeviceId);
            this.IsInitialized = true;
            this.LogStartSuccess();
        }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            var result = new StringBuilder();
            if (!this.IsInitialized)
            {
                throw new System.Exception("Must initialize device");
            }

            result.AppendFormat("Device Id: {0}\r\n", this.DeviceId);
            result.AppendFormat("File Descriptor: {0}\r\n", this.DeviceId);
            result.AppendFormat("Chip Id: {0}\r\n", this.Read(REGID));
            return result.ToString();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var sensorResult = new SensorSample(this.SensorName);

            // Initialize for reading
            this.Device.Write(new byte[] { REGCONTROLHUM, OVERSAMPLEHUM });
            var control = GetControlCode();

            this.Device.Write(new byte[] { REGCONTROL, (byte)control });

            // Read calibaration data
            var cal1 = this.ReadBytes(0x88, 24);
            var cal2 = this.ReadBytes(0xA1, 1);
            var cal3 = this.ReadBytes(0xE1, 7);

            sensorResult.AddDiagnostic("Calibration data (1/2/3)");
            sensorResult.AddDiagnostic(ByteOperations.PrintByteArray(cal1));
            sensorResult.AddDiagnostic(ByteOperations.PrintByteArray(cal2));
            sensorResult.AddDiagnostic(ByteOperations.PrintByteArray(cal3));

            var sensorData = ExtractSensorData(cal1, cal2, cal3);

            // Pause per spec
            var wait_time = GetWaitTime();
            Thread.Sleep((int)wait_time / 1000);

            // Read raw data
            var data = this.ReadBytes(REGDATA, 8);

            sensorResult.AddDiagnostic("Raw data");
            sensorResult.AddDiagnostic(ByteOperations.PrintByteArray(data));

            var pres_raw = (data[0] << 12) | (data[1] << 4) | (data[2] >> 4);
            var temp_raw = (data[3] << 12) | (data[4] << 4) | (data[5] >> 4);
            var hum_raw = (data[6] << 8) | data[7];

            sensorResult.AddDiagnostic("Raw readings");
            sensorResult.AddDiagnostic($"{pres_raw}:{temp_raw}:{hum_raw}");

            sensorResult.AddIntermediateObservation(temp_raw, "RAW TEMPERATURE", ObservationUnits.DegreesCelcius);
            sensorResult.AddIntermediateObservation(pres_raw, "RAW PRESSURE", ObservationUnits.HectoPascal);
            sensorResult.AddIntermediateObservation(hum_raw, "RAW HUMIDITY", ObservationUnits.Percentage);
            int tempPart1 = GetTempPart1(sensorData[SensorDigit.DigitT1], sensorData[SensorDigit.DigitT2], temp_raw);
            var tempPart2 = GetTempPart2(sensorData[SensorDigit.DigitT1], sensorData[SensorDigit.DigitT3], temp_raw);
            sensorResult.AddDiagnostic($"var1: {tempPart1}");
            sensorResult.AddDiagnostic($"var2: {tempPart2}");

            var t_fine = tempPart1 + tempPart2;
            sensorResult.AddDiagnostic($"tfine: {t_fine}");

            int finalTemp = ((t_fine * 5) + 128) >> 8;
            sensorResult.AddDiagnostic(string.Format("finaltemp: {0}", finalTemp));

            double temperatureValue = finalTemp / 100.0f;
            double rawTemperature = temperatureValue;

            // Refine pressure and adjust for temperature
            float varHum1 = (t_fine / 2.0f) - 64000.0f;
            float varHum2 = varHum1 * varHum1 * sensorData[SensorDigit.DigitP6] / 32768.0f;
            varHum2 += varHum1 * sensorData[SensorDigit.DigitP5] * 2.0f;
            varHum2 = (varHum2 / 4.0f) + (sensorData[SensorDigit.DigitP4] * 65536.0f);
            varHum1 = ((sensorData[SensorDigit.DigitP3] * varHum1 * varHum1 / 524288.0f) + (sensorData[SensorDigit.DigitP2] * varHum1)) / 524288.0f;
            varHum1 = (1.0f + (varHum1 / 32768.0f)) * sensorData[SensorDigit.DigitP1];

            double pressureValue;
            if (varHum1 == 0)
            {
                pressureValue = 0;
            }
            else
            {
                pressureValue = 1048576.0f - pres_raw;
                pressureValue = ((pressureValue - (varHum2 / 4096.0f)) * 6250.0f) / varHum1;
                var k = sensorData[SensorDigit.DigitP9] * pressureValue * pressureValue / 2147483648.0f;
                var l = pressureValue * sensorData[SensorDigit.DigitP8] / 32768.0f;
                pressureValue += (k + l + sensorData[SensorDigit.DigitP7]) / 16.0f;
                pressureValue /= 100;
            }

            // Refine humidity
            double humidityValue = t_fine - 76800.0f;
            humidityValue = (hum_raw - ((sensorData[SensorDigit.DigitH4] * 64.0f) +
                       ((sensorData[SensorDigit.DigitH5] / 16384.0f) * humidityValue))) *
                       ((sensorData[SensorDigit.DigitH2] / 65536.0f) *
                       (1.0f + ((sensorData[SensorDigit.DigitH6] / 67108864.0f) * humidityValue *
                       (1.0f + ((sensorData[SensorDigit.DigitH3] / 67108864.0f) * humidityValue)))));

            humidityValue *= 1.0f - (sensorData[SensorDigit.DigitH1] * humidityValue / 524288.0f);

            if (humidityValue > 100)
            {
                humidityValue = 100;
            }

            if (humidityValue < 0)
            {
                humidityValue = 0;
            }

            sensorResult.AddFinalObservation(temperatureValue + this.calibrationOffset, "TEMPERATURE", ObservationUnits.DegreesCelcius);
            sensorResult.AddFinalObservation(pressureValue, "PRESSURE", ObservationUnits.HectoPascal);
            sensorResult.AddFinalObservation(humidityValue, "HUMIDITY", ObservationUnits.Percentage);

            var pressure = new Pressure(pressureValue, UnitsNet.Units.PressureUnit.Hectopascal);
            var humidity = new RelativeHumidity(humidityValue, UnitsNet.Units.RelativeHumidityUnit.Percent);
            var humidityRatio = new Ratio(humidityValue, UnitsNet.Units.RatioUnit.Percent);
            var temperature = new Temperature(rawTemperature, UnitsNet.Units.TemperatureUnit.DegreeCelsius);

            var actualAltitude = new Length(this.altitudeInMeters, UnitsNet.Units.LengthUnit.Meter);
            var altitudeCalculated = WeatherHelper.CalculateAltitude(pressure);

            double absHumidity = WeatherHelper.CalculateAbsoluteHumidity(temperature, humidityRatio).GramsPerCubicMeter;
            double dewPoint = WeatherHelper.CalculateDewPoint(temperature, humidityRatio).DegreesCelsius;
            double heatIndex = WeatherHelper.CalculateHeatIndex(temperature, humidityRatio).DegreesCelsius;
            double vapourPressure = WeatherHelper.CalculateActualVaporPressure(temperature, humidityRatio).Hectopascals;
            double barometricPressure = WeatherHelper.CalculateBarometricPressure(pressure, temperature, actualAltitude, humidityRatio).Hectopascals;
            double vapourPressureOverIce = WeatherHelper.CalculateSaturatedVaporPressureOverIce(temperature).Hectopascals;
            double vapourPressureOverWater = WeatherHelper.CalculateSaturatedVaporPressureOverWater(temperature).Hectopascals;
            double seaLevelPressure = WeatherHelper.CalculateSeaLevelPressure(pressure, actualAltitude, temperature).Hectopascals;

            sensorResult.AddFinalObservation(temperature.DegreesCelsius + this.calibrationOffset, "TEMPERATURE", ObservationUnits.DegreesCelcius);
            sensorResult.AddFinalObservation(pressure.Hectopascals, "PRESSURE", ObservationUnits.HectoPascal);
            sensorResult.AddFinalObservation(barometricPressure, "BAROMETRIC PRESSURE", ObservationUnits.HectoPascal);
            sensorResult.AddFinalObservation(seaLevelPressure, "SEA LEVEL PRESSURE", ObservationUnits.HectoPascal);
            sensorResult.AddFinalObservation(vapourPressureOverIce, "OVER ICE PRESSURE", ObservationUnits.HectoPascal);
            sensorResult.AddFinalObservation(vapourPressureOverWater, "OVER WATER PRESSURE", ObservationUnits.HectoPascal);
            sensorResult.AddFinalObservation(humidity.Percent, "RELATIVE HUMIDITY", ObservationUnits.Percentage);
            sensorResult.AddFinalObservation(absHumidity, "ABSOLUTE HUMIDITY", ObservationUnits.GramsPerCubicMeter);
            sensorResult.AddFinalObservation(vapourPressure, "VAPOUR PRESSURE", ObservationUnits.Percentage);
            sensorResult.AddFinalObservation(altitudeCalculated.Meters, "CALCULATED ALTITUDE", ObservationUnits.Meters);
            sensorResult.AddFinalObservation(actualAltitude.Meters, "ACTUAL ALTITUDE", ObservationUnits.Meters);
            sensorResult.AddFinalObservation(heatIndex, "HEAT INDEX", ObservationUnits.DegreesCelcius);
            sensorResult.AddFinalObservation(dewPoint, "DEW POINT", ObservationUnits.DegreesCelcius);

            observation.Temperature1 = temperature.DegreesCelsius + this.calibrationOffset;
            observation.Pressure = pressure.Hectopascals;
            observation.BarometricPressure = barometricPressure;
            observation.SealevelPressure = seaLevelPressure;
            observation.OverIcePressure = vapourPressureOverIce;
            observation.OverWaterPressure = vapourPressureOverWater;
            observation.RelativeHumidity = humidity.Percent;
            observation.AbsoluteHumidity = absHumidity;
            observation.ActualAltitude = actualAltitude.Meters;
            observation.CalculatedAltitude = altitudeCalculated.Meters;
            observation.HeatIndex = heatIndex;
            observation.DewPoint = dewPoint;

            this.LogTakeReadingComplete();

            return sensorResult;
        }

        /// <summary>
        /// Get the control code from the settings.
        /// </summary>
        /// <returns>The control code to apply.</returns>
        private static int GetControlCode()
        {
            return OVERSAMPLETEMP << 5 | OVERSAMPLEPRES << 2 | MODE;
        }
    }
}
