// <copyright file="RainGauge.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Device.Gpio;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;
    using HollowMan.Core.Shared;

    /// <summary>
    /// A switch based rainguage implementation.
    /// </summary>
    public class RainGauge : Sensor
    {
        private const double BUCKETSIZE = 0.2794;

        private readonly int pin;
        private readonly GpioController gpioController;
        private readonly InterlockedCounter tipCount = new InterlockedCounter();

        /// <summary>
        /// Initializes a new instance of the <see cref="RainGauge"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pin">The GPIO pin config.</param>
        public RainGauge(SensorController controller, int pin)
        {
            this.SensorName = "RainGaugeGPIO_HMDriver";
            this.Controller = controller;
            this.gpioController = controller?.GPIOController ?? throw new ArgumentNullException(nameof(controller));
            this.pin = pin;
            this.DeviceId = (byte)(10000 + pin);
            this.gpioController.OpenPin(pin);
            this.gpioController.SetPinMode(pin, PinMode.InputPullUp);
            this.gpioController.RegisterCallbackForPinValueChangedEvent(this.pin, PinEventTypes.Falling, this.HandleCount);
            this.IsInitialized = true;
        }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();
            this.LogStartSuccess();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var sensorResult = new SensorSample(this.SensorName);
            int count = this.tipCount.GetAndReset();
            this.LogMessage($"Tick Count: {count}");
            sensorResult.AddFinalObservation(count * BUCKETSIZE, "PRECIPITATION", ObservationUnits.Millimeter);
            observation.Precipitation = count * BUCKETSIZE;
            this.LogTakeReadingComplete();
            return sensorResult;
        }

        private void HandleCount(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            this.tipCount.Increment();
        }
    }
}
