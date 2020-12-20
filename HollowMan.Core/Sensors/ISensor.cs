// <copyright file="ISensor.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors
{
    using System.Device.I2c;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;

    /// <summary>
    /// Sensor interface.
    /// </summary>
    public interface ISensor
    {
        /// <summary>
        /// Gets the underlying I2c device.
        /// </summary>
        I2cDevice Device { get; }

        /// <summary>
        /// Gets the devuce id.
        /// </summary>
        byte DeviceId { get; }

        /// <summary>
        /// Gets a value indicating whether the device is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the sensor name.
        /// </summary>
        public string SensorName { get; }

        /// <summary>
        /// Gets the sensor controller.
        /// </summary>
        SensorController Controller { get; }

        /// <summary>
        /// Gets the diagnostic information for console debugging.
        /// </summary>
        /// <returns>Current diagnostic state as string.</returns>
        string GetDiagnostics();

        /// <summary>
        /// Intialize the device.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Take a sensor reading.
        /// </summary>
        /// <param name="observation">The observation to update.</param>
        /// <returns>A sensor sample for the current state.</returns>
        ISensorSample TakeReading(WeatherObservation observation);
    }
}