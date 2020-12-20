// <copyright file="VEML6075.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Device.I2c;
    using System.Threading;
    using System.Threading.Tasks;
    using HollowMan.Core.SensorData;

    /// <summary>
    /// VEML6075 driver for volatikes.
    /// </summary>
    public class VEML6075 : Sensor, IDisposable
    {
        private const int I2CADDRESS = 0x10;
        private static VEML6075Async veml;
        private readonly object syncroot;
        private float uvacalc;
        private float uvbcalc;
        private float uvicalc;
        private Timer sensorTimer;
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VEML6075"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        public VEML6075(SensorControllers.SensorController controller)
        {
            this.DeviceId = I2CADDRESS;
            this.SensorName = "VEML6075_HMDriver";
            this.syncroot = new object();
            this.Controller = controller;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();
            this.Device = this.Controller.GetI2CConnection(this.DeviceId);
            this.LogMessage($"Device address: {this.Device.ConnectionSettings.DeviceAddress}");
            this.AsyncInitialize();
            this.IsInitialized = true;
            this.LogStartSuccess();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var sensorResult = new SensorSample(this.SensorName);

            lock (this.syncroot)
            {
                sensorResult.AddFinalObservation(this.uvacalc, "UVA", ObservationUnits.Default);
                sensorResult.AddFinalObservation(this.uvbcalc, "UVB", ObservationUnits.Default);
                sensorResult.AddFinalObservation(this.uvicalc, "UVI", ObservationUnits.Default);
                observation.UvA = this.uvacalc;
                observation.UvB = this.uvbcalc;
                observation.UvI = this.uvicalc;
            }

            this.LogTakeReadingComplete();
            return sensorResult;
        }

        /// <summary>
        /// Dispose of the underlying resources.
        /// </summary>
        /// <param name="disposing">Are we currently disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.sensorTimer.Dispose();
                }

                this.disposedValue = true;
            }
        }

        private async void AsyncInitialize()
        {
            // Initialize and configure sensor
            await this.InitializeVEML6075().ConfigureAwait(true);

            // Configure timer to 2000ms delayed start and 2000ms interval
            this.sensorTimer = new Timer(new TimerCallback(this.SensorTimerTick), null, 2000, 2000);
        }

        private async Task InitializeVEML6075()
        {
            // Create sensor instance
            veml = new VEML6075Async(this);

            // Advanced sensor configuration
            var result = await veml.Config(
                VEML6075Async.IntegrationTime.IT800ms,
                VEML6075Async.DynamicSetting.High,
                VEML6075Async.Trigger.NoActiveForceTrigger,
                VEML6075Async.ActiveForceMode.NormalMode,
                VEML6075Async.VEMLPowerMode.PowerOn).ConfigureAwait(true);

            this.LogMessage($"VEML Ready? {result}");
        }

        private void SensorTimerTick(object state)
        {
            lock (this.syncroot)
            {
                this.uvacalc = (float)veml.CalculateCompensatedUVA();
                this.uvbcalc = (float)veml.CalculateCompensatedUVB();
                this.uvicalc = (float)veml.CalculateAverageUVIndex();
            }
        }
    }
}