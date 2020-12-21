// <copyright file="Anemometer.cs" company="Paul Baecke">
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
    /// A switch based anemometer implementation.
    /// </summary>
    public class Anemometer : Sensor
    {
        private const int GUSTLENGTHINSEC = 5;
        private const double CIRCUMFERENCE = 9.0 / 1000.0;
        private const double CALIB = 2.36;

        private readonly int pin;
        private readonly GpioController gpioController;
        private readonly InterlockedCounter countSinceLastRead = new InterlockedCounter();
        private readonly InterlockedCounter gustCount = new InterlockedCounter();

        private DateTime startTime;
        private DateTime lastObservation;

        private double maxGust;

        /// <summary>
        /// Initializes a new instance of the <see cref="Anemometer"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pin">The GPIO pin config.</param>
        public Anemometer(SensorController controller, int pin)
        {
            this.SensorName = "AnemometerGPIO_HMDriver";
            this.Controller = controller;
            this.gpioController = controller?.GPIOController ?? throw new ArgumentNullException(nameof(controller));
            this.pin = pin;
            this.DeviceId = (byte)(10000 + pin);
            this.gpioController.OpenPin(pin);
            this.gpioController.SetPinMode(pin, PinMode.InputPullUp);
            this.gpioController.RegisterCallbackForPinValueChangedEvent(this.pin, PinEventTypes.Rising, this.HandleCount);
            this.IsInitialized = true;

            // Register the startup time
            this.startTime = DateTime.Now;
            this.lastObservation = DateTime.Now;

            // initialize counters
            this.maxGust = 0;
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
            var obsTime = DateTime.Now;
            double elapsedSinceReading = (obsTime - this.startTime).TotalSeconds;
            int count = this.countSinceLastRead.GetAndReset();
            this.LogMessage($"Elapsed: {elapsedSinceReading}");
            this.LogMessage($"Tick Count: {count}");
            double average = CalculateSpeed(count, elapsedSinceReading);

            sensorResult.AddFinalObservation(this.maxGust, "WIND_GUST", ObservationUnits.KmPerHour);
            sensorResult.AddFinalObservation(average, "WIND_AVERAGE", ObservationUnits.KmPerHour);

            observation.WindAverage = average;
            observation.WindGust = this.maxGust != 1 ? this.maxGust : average;

            this.startTime = obsTime;
            this.gustCount.Reset();
            this.maxGust = -1;

            this.LogTakeReadingComplete();

            return sensorResult;
        }

        private static double CalculateSpeed(int count, double timeInSeconds)
        {
            double rotations = count / 2.0;
            double distance = (CIRCUMFERENCE * rotations) / timeInSeconds;
            double speed = distance * 3600.0;
            return speed * CALIB;
        }

        private void HandleCount(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            var obsTime = DateTime.Now;
            this.countSinceLastRead.Increment();
            double gustElapsed = (obsTime - this.lastObservation).TotalSeconds;
            if (gustElapsed >= GUSTLENGTHINSEC)
            {
                // calculate gust
                this.gustCount.Increment();
                this.maxGust = Math.Max(this.maxGust, CalculateSpeed(this.gustCount.GetAndReset(), gustElapsed));
                this.lastObservation = obsTime;
                this.gustCount.Reset();
            }

            this.gustCount.Increment();
        }
    }
}
