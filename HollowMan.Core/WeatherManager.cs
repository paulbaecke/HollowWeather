// <copyright file="WeatherManager.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Timers;
    using HollowMan.Core.DB;
    using HollowMan.Core.Logging;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.Sensors;
    using HollowMan.Core.Sensors.Drivers;

    /// <summary>
    /// Config driven sensor manager.
    /// Work in progress.
    /// </summary>
    public class WeatherManager : IDisposable
    {
        private readonly List<ISensor> activeSensors;
        private readonly SensorController sensorController;
        private readonly int interval;
        private readonly int altitudeInMeters;
        private readonly double correctionFactor;
        private readonly EventLogger logger;
        private bool isDisposed;
        private IList<IWeatherLogger> weatherLoggers;
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherManager"/> class.
        /// </summary>
        /// <param name="intervalInSeconds">Polling interval in seconds.</param>
        /// <param name="altitudeInMeters">Altitude of location in meters. Used for corrections and calculating metrics.</param>
        /// <param name="correctionFactor">Correction factor for BME280 chip if needed.</param>
        public WeatherManager(int intervalInSeconds, int altitudeInMeters, double correctionFactor = 0)
        {
            this.interval = intervalInSeconds;
            this.logger = new EventLogger();
            this.altitudeInMeters = altitudeInMeters;
            this.correctionFactor = correctionFactor;

            this.sensorController = new SensorController(this.logger);
            this.activeSensors = new List<ISensor>();
            this.RegisterI2cAddresses();

            this.AddSensors(this.sensorController);
            this.InitializeSensors();
            this.AddWeatherLoggers();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WeatherManager"/> class.
        /// </summary>
        ~WeatherManager()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start observing weather.
        /// </summary>
        public void StartObserving()
        {
            this.isRunning = true;
            while (this.isRunning)
            {
                if (this.TryGetObservation(out IWeatherObservation observation))
                {
                    foreach (var weatherlogger in this.weatherLoggers)
                    {
                        weatherlogger.Log(observation);
                    }
                }

                Thread.Sleep(this.interval * 1000);
            }
        }

        /// <summary>
        /// Stop observing.
        /// </summary>
        public void Stop()
        {
            this.isRunning = false;
        }

        /// <summary>
        /// Try to take an observation from all sensors.
        /// Cannot execute only one instance of this.
        /// </summary>
        /// <param name="observation">The current weather state.</param>
        /// <returns>True if successfull.</returns>
        public bool TryGetObservation(out IWeatherObservation observation)
        {
            observation = null;

            try
            {
                this.logger.LogSystemStart("Taking readings.");
                observation = new WeatherObservation();

                foreach (var sensor in this.activeSensors)
                {
                    var data = sensor.TakeReading(observation as WeatherObservation);
                    this.logger.LogWeatherObservation(data);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogSystemError(ex.ToString());
                return false;
            }

            this.logger.LogSystemSuccess("Taking readings.");
            return true;
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
                foreach (var sensor in this.activeSensors)
                {
                    if (sensor is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                this.sensorController.Dispose();
            }

            this.isDisposed = true;
        }

        private void RegisterI2cAddresses()
        {
            this.logger.LogSystemStart("Registering I2c");

            // CSS811
            this.sensorController.AddDevice(0x77);

            // BME280
            this.sensorController.AddDevice(0x5b);

            // VEML6075
            this.sensorController.AddDevice(0x10);

            this.logger.LogSystemSuccess("Registering I2c");
        }

        private void AddSensors(SensorController controller)
        {
            // Add sensors
            this.logger.LogSystemStart("Adding sensors");
            this.activeSensors.Add(new BME280(controller, this.correctionFactor, this.altitudeInMeters));
            this.activeSensors.Add(new CCS811IoT(controller));
            this.activeSensors.Add(new VEML6075(controller));
            this.activeSensors.Add(new DS18B20(controller));
            this.activeSensors.Add(new Anemometer(controller, 5));
            this.activeSensors.Add(new RainGauge(controller, 6));
            this.logger.LogSystemSuccess("Adding sensors");
        }

        private void InitializeSensors()
        {
            this.logger.LogSystemStart("Initialize sensors");
            foreach (var sensor in this.activeSensors)
            {
                sensor.Initialize();
            }

            this.logger.LogSystemSuccess("Initialize sensors");
        }

        private void AddWeatherLoggers()
        {
            this.logger.LogSystemStart("Init weather loggers");

            // this needs to move to config
            this.weatherLoggers = new List<IWeatherLogger>
            {
                new PrometheusLogger(this.logger,  "192.168.1.152", "http://192.168.1.128"),
            };

            this.logger.LogSystemSuccess("Init weather loggers");
        }
    }
}