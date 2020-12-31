// <copyright file="WeatherObservation.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core
{
    using System;

    /// <summary>
    /// Represents a weather observation taken from sensor readings.
    /// </summary>
    public class WeatherObservation : IWeatherObservation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherObservation"/> class.
        /// </summary>
        public WeatherObservation()
        {
            this.TimeStamp = DateTime.Now;
        }

        /// <inheritdoc/>
        public double Temperature1 { get; internal set; }

        /// <inheritdoc/>
        public double Temperature2 { get; internal set; }

        /// <inheritdoc/>
        public double UvA { get; internal set; }

        /// <inheritdoc/>
        public double UvB { get; internal set; }

        /// <inheritdoc/>
        public double UvI { get; internal set; }

        /// <inheritdoc/>
        public double TVOC { get; internal set; }

        /// <inheritdoc/>
        public double CO2 { get; internal set; }

        /// <inheritdoc/>
        public double Pressure { get; internal set; }

        /// <inheritdoc/>
        public double BarometricPressure { get; internal set; }

        /// <inheritdoc/>
        public double SealevelPressure { get; internal set; }

        /// <inheritdoc/>
        public double OverIcePressure { get; internal set; }

        /// <inheritdoc/>
        public double OverWaterPressure { get; internal set; }

        /// <inheritdoc/>
        public double RelativeHumidity { get; internal set; }

        /// <inheritdoc/>
        public double AbsoluteHumidity { get; internal set; }

        /// <inheritdoc/>
        public double VapourPressure { get; internal set; }

        /// <inheritdoc/>
        public double CalculatedAltitude { get; internal set; }

        /// <inheritdoc/>
        public double ActualAltitude { get; internal set; }

        /// <inheritdoc/>
        public double HeatIndex { get; internal set; }

        /// <inheritdoc/>
        public double DewPoint { get; internal set; }

        /// <inheritdoc />
        public double WindGust { get; internal set; }

        /// <inheritdoc />
        public double WindAverage { get; internal set; }

        /// <inheritdoc />
        public double WindDirection { get; internal set; }

        /// <inheritdoc/>
        public DateTime TimeStamp { get; private set; }

        /// <inheritdoc />
        public double Precipitation { get; internal set; }
    }
}