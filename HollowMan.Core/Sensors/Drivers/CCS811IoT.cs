// <copyright file="CCS811IoT.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Collections.Generic;
    using System.Device.I2c;
    using System.Text;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;
    using Iot.Device.Ccs811;
    using UnitsNet;

    /// <summary>
    /// The new improved BME280IoT driver based on the Iot library.
    /// BME280IoT provider temperature, humidity and pressure.
    /// </summary>
    public class CCS811IoT : Sensor, IDisposable
    {
        private const byte DEVICEID = 0x5b;
        private I2cDevice i2cDevice;
        private Ccs811Sensor sensor;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CCS811IoT"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        public CCS811IoT(SensorController controller)
            : this(controller, DEVICEID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CCS811IoT"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        /// <param name="deviceId">Override the default device id.</param>
        public CCS811IoT(SensorController controller, byte deviceId)
        {
            this.Controller = controller;
            this.DeviceId = deviceId;
            this.Messages = new List<string>();
            this.SensorName = "CCS811_IoTDriver";
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CCS811IoT"/> class.
        /// </summary>
        ~CCS811IoT()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the list of messages.
        /// </summary>
        public IList<string> Messages { get; private set; }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();
            this.i2cDevice = this.Controller.GetI2CConnection(this.DeviceId);
            this.sensor = new Ccs811Sensor(this.i2cDevice)
            {
                OperationMode = OperationMode.Idle,
            };

            this.LogMessage($"Hardware identification: 0x{this.sensor.HardwareIdentification:X2}, must be 0x81");
            this.LogMessage($"Hardware version: 0x{this.sensor.HardwareVersion:X2}, must be 0x1X where any X is valid");
            this.LogMessage($"Application version: {this.sensor.ApplicationVersion}");
            this.LogMessage($"Boot loader version: {this.sensor.BootloaderVersion}");
            var baseline = this.sensor.BaselineAlgorithmCalculation;
            this.LogMessage($"Baseline calculation value: {baseline}, changing baseline");
            this.sensor.BaselineAlgorithmCalculation = 50300;
            this.LogMessage($"Baseline calculation value: {this.sensor.BaselineAlgorithmCalculation}, changing baseline for the previous one");
            this.sensor.BaselineAlgorithmCalculation = baseline;
            this.LogMessage($"Baseline calculation value: {this.sensor.BaselineAlgorithmCalculation}");
            this.sensor.OperationMode = OperationMode.ConstantPower1Second;
            this.IsInitialized = true;
            this.LogStartSuccess();
        }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            var result = new StringBuilder();
            if (!this.IsInitialized)
            {
                throw new Exception("Must initialize device");
            }

            result.AppendLine($"Device Id: {this.DeviceId}");
            result.AppendLine($"File Descriptor: {this.DeviceId}");
            result.AppendLine($"Chip Id: {this.sensor.HardwareVersion}");
            result.AppendLine($"Hardware identification: 0x{this.sensor.HardwareIdentification:X2}, must be 0x81");
            result.AppendLine($"Hardware version: 0x{this.sensor.HardwareVersion:X2}, must be 0x1X where any X is valid");
            result.AppendLine($"Application version: {this.sensor.ApplicationVersion}");
            result.AppendLine($"Boot loader version: {this.sensor.BootloaderVersion}");
            var baseline = this.sensor.BaselineAlgorithmCalculation;
            result.AppendLine($"Baseline calculation value: {baseline}, changing baseline");
            this.sensor.BaselineAlgorithmCalculation = 50300;
            result.AppendLine($"Baseline calculation value: {this.sensor.BaselineAlgorithmCalculation}, changing baseline for the previous one");
            this.sensor.BaselineAlgorithmCalculation = baseline;
            result.AppendLine($"Baseline calculation value: {this.sensor.BaselineAlgorithmCalculation}");
            return result.ToString();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var sensorResult = new SensorSample(this.SensorName);
            if (this.sensor.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc))
            {
                sensorResult.AddFinalObservation(eCO2.PartsPerMillion, "CO2", ObservationUnits.PartsPerMillion);
                sensorResult.AddFinalObservation(eTVOC.PartsPerBillion, "TVOC", ObservationUnits.PartsPerBillion);
                observation.CO2 = eCO2.PartsPerMillion;
                observation.TVOC = eTVOC.PartsPerBillion;
            }
            else
            {
                this.LogError("Error reading from CSS811");
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
