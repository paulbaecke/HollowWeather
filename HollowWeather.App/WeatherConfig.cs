// <copyright file="WeatherConfig.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>
namespace HollowWeather.App
{
    /// <summary>
    /// The weather configuration.
    /// </summary>
    public class WeatherConfig
    {
        /// <summary>
        /// Gets or sets the daemon name.
        /// </summary>
        public string DaemonName { get; set; }

        /// <summary>
        /// Gets or sets the logging interval in seconds.
        /// </summary>
        public int LoggingInterval { get; set; }

        /// <summary>
        /// Gets or sets the altitude in meters.
        /// </summary>
        public int Altitude { get; set; }

        /// <summary>
        /// Gets or sets the BME280 correction.
        /// </summary>
        public double Correction { get; set; }
    }
}
