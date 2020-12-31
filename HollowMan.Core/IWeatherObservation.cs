// <copyright file="IWeatherObservation.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core
{
    using System;

    /// <summary>
    /// Summary of current weather status.
    /// </summary>
    public interface IWeatherObservation
    {
        /// <summary>
        /// Gets the observation timestamp.
        /// </summary>
        DateTime TimeStamp { get; }

        /// <summary>
        /// Gets humidity in %age.
        /// </summary>
        double AbsoluteHumidity { get; }

        /// <summary>
        /// Gets altitude as set.
        /// </summary>
        double ActualAltitude { get; }

        /// <summary>
        /// Gets calculated barometric pressure.
        /// </summary>
        double BarometricPressure { get; }

        /// <summary>
        /// Gets calculated altitude.
        /// </summary>
        double CalculatedAltitude { get; }

        /// <summary>
        /// Gets CO2.
        /// </summary>
        double CO2 { get; }

        /// <summary>
        /// Gets calculated dewpoint.
        /// </summary>
        double DewPoint { get; }

        /// <summary>
        /// Gets calculated heatindex.
        /// </summary>
        double HeatIndex { get; }

        /// <summary>
        /// Gets calculated over ice pressure.
        /// </summary>
        double OverIcePressure { get; }

        /// <summary>
        /// Gets calculatedo over water pressure.
        /// </summary>
        double OverWaterPressure { get; }

        /// <summary>
        /// Gets raw pressure.
        /// </summary>
        double Pressure { get; }

        /// <summary>
        /// Gets relative humidity.
        /// </summary>
        double RelativeHumidity { get; }

        /// <summary>
        /// Gets sea level equivalent pressure.
        /// </summary>
        double SealevelPressure { get; }

        /// <summary>
        /// Gets temperature from first sensor.
        /// </summary>
        double Temperature1 { get; }

        /// <summary>
        /// Gets temperature from second sensor.
        /// </summary>
        double Temperature2 { get; }

        /// <summary>
        /// Gets organic volatiles.
        /// </summary>
        double TVOC { get; }

        /// <summary>
        /// Gets UVA.
        /// </summary>
        double UvA { get; }

        /// <summary>
        /// Gets UVB.
        /// </summary>
        double UvB { get; }

        /// <summary>
        /// Gets UVI.
        /// </summary>
        double UvI { get; }

        /// <summary>
        /// Gets calculated vapour pressure.
        /// </summary>
        double VapourPressure { get; }

        /// <summary>
        /// Gets calculated wind gust.
        /// </summary>
        double WindGust { get; }

        /// <summary>
        /// Gets average windspeed.
        /// </summary>
        double WindAverage { get; }

        /// <summary>
        /// Gets the rainfall for the time period.
        /// </summary>
        double Precipitation { get; }

        /// <summary>
        /// Gets the wind direction.
        /// </summary>
        double WindDirection { get; }
    }
}