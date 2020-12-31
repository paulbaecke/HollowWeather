// <copyright file="BME280IoT.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Device.I2c;
    using System.Text;
    using System.Threading;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;
    using Iot.Device.Bmxx80;
    using Iot.Device.Bmxx80.FilteringMode;
    using Iot.Device.Common;
    using UnitsNet;

    /// <summary>
    /// The new improved BME280IoT driver based on the Iot library.
    /// BME280IoT provider temperature, humidity and pressure.
    /// </summary>
    public class BME280IoT : Sensor, IDisposable
    {
        private const byte DEVICEID = 0x77;
        private readonly int altitudeInMeters;
        private I2cDevice i2cDevice;
        private double calibrationOffset;
        private Bme280 sensor;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BME280IoT"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        /// <param name="calibrationOffset">The overheating correction to use.</param>
        /// <param name="altitudeInMeters">Altitude of location in meters. Used for corrections and calculating metrics.</param>
        public BME280IoT(SensorController controller, double calibrationOffset, int altitudeInMeters)
            : this(controller, calibrationOffset, altitudeInMeters, DEVICEID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BME280IoT"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        /// <param name="calibrationOffset">The overheating correction to use.</param>
        /// <param name="altitudeInMeters">Altitude of location in meters. Used for corrections and calculating metrics.</param>
        /// <param name="deviceId">Override the default device id.</param>
        public BME280IoT(SensorController controller, double calibrationOffset, int altitudeInMeters, byte deviceId)
        {
            this.DeviceId = deviceId;
            this.SensorName = "BME280_IoTDriver";
            this.Controller = controller;
            this.calibrationOffset = calibrationOffset;
            this.altitudeInMeters = altitudeInMeters;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BME280IoT"/> class.
        /// </summary>
        ~BME280IoT()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();
            this.i2cDevice = this.Controller.GetI2CConnection(this.DeviceId);
            this.sensor = new Bme280(this.i2cDevice)
            {
                TemperatureSampling = Sampling.LowPower,
                PressureSampling = Sampling.UltraHighResolution,
                HumiditySampling = Sampling.Standard,
            };

            var readResult = this.sensor.ReadStatus();
            this.LogMessage($"BME280 status {readResult}");
            var result = this.sensor.Read();
            if (result.Humidity.HasValue && result.Pressure.HasValue && result.Temperature.HasValue)
            {
                var pressure = result.Pressure.Value;
                var temperature = new Temperature(result.Temperature.Value.DegreesCelsius - this.calibrationOffset, UnitsNet.Units.TemperatureUnit.DegreeCelsius);
                var humidity = result.Humidity.Value;

                var actualAltitude = new Length(this.altitudeInMeters, UnitsNet.Units.LengthUnit.Meter);
                var calculatedAltitude = WeatherHelper.CalculateAltitude(pressure, WeatherHelper.MeanSeaLevel, temperature);

                double absHumidity = WeatherHelper.CalculateAbsoluteHumidity(temperature, humidity).GramsPerCubicMeter;
                double dewPoint = WeatherHelper.CalculateDewPoint(temperature, humidity).DegreesCelsius;
                double heatIndex = WeatherHelper.CalculateHeatIndex(temperature, humidity).DegreesCelsius;
                double vapourPressure = WeatherHelper.CalculateActualVaporPressure(temperature, humidity).Hectopascals;
                double barometricPressure = WeatherHelper.CalculateBarometricPressure(pressure, temperature, actualAltitude, humidity).Hectopascals;
                double vapourPressureOverIce = WeatherHelper.CalculateSaturatedVaporPressureOverIce(temperature).Hectopascals;
                double vapourPressureOverWater = WeatherHelper.CalculateSaturatedVaporPressureOverWater(temperature).Hectopascals;
                double seaLevelPressure = WeatherHelper.CalculateSeaLevelPressure(pressure, actualAltitude, temperature).Hectopascals;

                this.LogMessage($"Temperature: {temperature.DegreesCelsius:0.#}\u00B0C");
                this.LogMessage($"Pressure: {pressure.Hectopascals:0.##}hPa");
                this.LogMessage($"Barometric Pressure: {barometricPressure:0.##}hPa");
                this.LogMessage($"Sea level Pressure: {seaLevelPressure:0.##}hPa");
                this.LogMessage($"Over ice Pressure: {vapourPressureOverIce:0.##}hPa");
                this.LogMessage($"Over water Pressure: {vapourPressureOverWater:0.##}hPa");
                this.LogMessage($"Relative humidity: {humidity.Percent:0.#}%");
                this.LogMessage($"Absolute humidity: {absHumidity:0.#}g/m3");
                this.LogMessage($"Vapour pressure: {vapourPressure:0.#}hPa");
                this.LogMessage($"Calculate altitude: {calculatedAltitude.Meters:0.##}m");
                this.LogMessage($"Actual altitude: {actualAltitude.Meters:0.##}m");
                this.LogMessage($"Heat index: {heatIndex:0.#}\u00B0C");
                this.LogMessage($"Dew point: {dewPoint:0.#}\u00B0C");

                Thread.Sleep(1000);

                // set sane defaults
                this.sensor.TemperatureSampling = Sampling.UltraLowPower;
                this.sensor.PressureSampling = Sampling.UltraLowPower;
                this.sensor.HumiditySampling = Sampling.UltraLowPower;
                this.sensor.FilterMode = Bmx280FilteringMode.Off;
                this.sensor.SetPowerMode(Iot.Device.Bmxx80.PowerMode.Bmx280PowerMode.Forced);

                this.IsInitialized = true;
                this.LogStartSuccess();
            }

            this.LogError("Couldn't read from BME280");
        }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            var result = new StringBuilder();
            if (!this.IsInitialized)
            {
                throw new Exception("Must initialize device");
            }

            result.AppendFormat("Device Id: {0}\r\n", this.DeviceId);
            result.AppendFormat("File Descriptor: {0}\r\n", this.DeviceId);
            result.AppendFormat("Chip Id: {0}\r\n", Bmx280Base.SecondaryI2cAddress);
            return result.ToString();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var sensorResult = new SensorSample(this.SensorName);

            var result = this.sensor.Read();
            if (result.Humidity.HasValue && result.Pressure.HasValue && result.Temperature.HasValue)
            {
                var pressure = result.Pressure.Value;
                var temperature = new Temperature(result.Temperature.Value.DegreesCelsius + this.calibrationOffset, UnitsNet.Units.TemperatureUnit.DegreeCelsius);
                var rawTemperature = result.Temperature.Value;
                var humidity = result.Humidity.Value;
                var actualAltitude = new Length(this.altitudeInMeters, UnitsNet.Units.LengthUnit.Meter);
                var calculatedAltitude = WeatherHelper.CalculateAltitude(pressure, WeatherHelper.MeanSeaLevel, rawTemperature);

                double absHumidity = WeatherHelper.CalculateAbsoluteHumidity(temperature, humidity).GramsPerCubicMeter;
                double dewPoint = WeatherHelper.CalculateDewPoint(temperature, humidity).DegreesCelsius;
                double heatIndex = WeatherHelper.CalculateHeatIndex(temperature, humidity).DegreesCelsius;
                double vapourPressure = WeatherHelper.CalculateActualVaporPressure(temperature, humidity).Hectopascals;
                double barometricPressure = WeatherHelper.CalculateBarometricPressure(pressure, temperature, actualAltitude, humidity).Hectopascals;
                double vapourPressureOverIce = WeatherHelper.CalculateSaturatedVaporPressureOverIce(temperature).Hectopascals;
                double vapourPressureOverWater = WeatherHelper.CalculateSaturatedVaporPressureOverWater(temperature).Hectopascals;
                double seaLevelPressure = WeatherHelper.CalculateSeaLevelPressure(pressure, actualAltitude, temperature).Hectopascals;

                sensorResult.AddFinalObservation(temperature.DegreesCelsius, "TEMPERATURE", ObservationUnits.DegreesCelcius);
                sensorResult.AddFinalObservation(pressure.Hectopascals, "PRESSURE", ObservationUnits.HectoPascal);
                sensorResult.AddFinalObservation(barometricPressure, "BAROMETRIC PRESSURE", ObservationUnits.HectoPascal);
                sensorResult.AddFinalObservation(seaLevelPressure, "SEA LEVEL PRESSURE", ObservationUnits.HectoPascal);
                sensorResult.AddFinalObservation(vapourPressureOverIce, "OVER ICE PRESSURE", ObservationUnits.HectoPascal);
                sensorResult.AddFinalObservation(vapourPressureOverWater, "OVER WATER PRESSURE", ObservationUnits.HectoPascal);
                sensorResult.AddFinalObservation(humidity.Percent, "RELATIVE HUMIDITY", ObservationUnits.Percentage);
                sensorResult.AddFinalObservation(absHumidity, "ABSOLUTE HUMIDITY", ObservationUnits.GramsPerCubicMeter);
                sensorResult.AddFinalObservation(vapourPressure, "VAPOUR PRESSURE", ObservationUnits.HectoPascal);
                sensorResult.AddFinalObservation(calculatedAltitude.Meters, "CALCULATED ALTITUDE", ObservationUnits.Meters);
                sensorResult.AddFinalObservation(actualAltitude.Meters, "ACTUAL ALTITUDE", ObservationUnits.Meters);
                sensorResult.AddFinalObservation(heatIndex, "HEAT INDEX", ObservationUnits.DegreesCelcius);
                sensorResult.AddFinalObservation(dewPoint, "DEW POINT", ObservationUnits.DegreesCelcius);

                observation.Temperature1 = temperature.DegreesCelsius;
                observation.Pressure = pressure.Hectopascals;
                observation.BarometricPressure = barometricPressure;
                observation.SealevelPressure = seaLevelPressure;
                observation.OverIcePressure = vapourPressureOverIce;
                observation.OverWaterPressure = vapourPressureOverWater;
                observation.RelativeHumidity = humidity.Percent;
                observation.AbsoluteHumidity = absHumidity;
                observation.ActualAltitude = actualAltitude.Meters;
                observation.CalculatedAltitude = calculatedAltitude.Meters;
                observation.HeatIndex = heatIndex;
                observation.DewPoint = dewPoint;
            }

            this.LogTakeReadingComplete();
            return sensorResult;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Currently disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.sensor.Dispose();
                this.i2cDevice.Dispose();
            }

            this.isDisposed = true;
        }
    }
}
