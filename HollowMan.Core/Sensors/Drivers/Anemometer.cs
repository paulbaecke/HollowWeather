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
        private const double RADIUSINCM = 9.0;
        private const double CALIB = 2.36;
        private readonly double circumference;
        private readonly int pin;
        private readonly GpioController gpioController;
        private int countSinceLastRead;
        private int gustCount;
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
            this.circumference = (2 * Math.PI * RADIUSINCM) / 100000;
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
            int count = this.countSinceLastRead;
            this.countSinceLastRead = 0;
            this.LogMessage($"Elapsed: {elapsedSinceReading}");
            this.LogMessage($"Tick Count: {count}");
            double average = this.CalculateSpeed(count, elapsedSinceReading);

            sensorResult.AddFinalObservation(this.maxGust, "WIND_GUST", ObservationUnits.KmPerHour);
            sensorResult.AddFinalObservation(average, "WIND_AVERAGE", ObservationUnits.KmPerHour);

            observation.WindAverage = average;
            observation.WindGust = this.maxGust != -1 ? this.maxGust : average;

            this.startTime = obsTime;
            this.gustCount = 0;
            this.maxGust = -1;

            this.LogTakeReadingComplete();

            return sensorResult;
        }

        private double CalculateSpeed(int count, double timeInSeconds)
        {
            double rotations = count / 2.0;
            double distanceInKm = this.circumference * rotations;
            double kmPerSecond = distanceInKm / timeInSeconds;
            double kmPerHour = kmPerSecond * 3600;
            return kmPerHour * CALIB;
        }

        private void HandleCount(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            var obsTime = DateTime.Now;
            this.countSinceLastRead++;
            double gustElapsed = (obsTime - this.lastObservation).TotalSeconds;
            if (gustElapsed >= GUSTLENGTHINSEC)
            {
                // calculate gust
                int count = this.gustCount;
                this.gustCount = 0;
                this.maxGust = Math.Max(this.maxGust, this.CalculateSpeed(count, gustElapsed));
                this.lastObservation = obsTime;
                this.gustCount = 0;
            }

            this.gustCount++;
        }
    }
}
