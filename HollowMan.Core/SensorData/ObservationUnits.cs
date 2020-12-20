// <copyright file="ObservationUnits.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.SensorData
{
    /// <summary>
    /// Represents the observation units. So we can remember what we're measuring.
    /// </summary>
    public enum ObservationUnits
    {
        /// <summary>
        /// Just a value.
        /// </summary>
        Default,

        /// <summary>
        /// Km per hour.
        /// </summary>
        KmPerHour,

        /// <summary>
        /// Degress celcius.
        /// </summary>
        DegreesCelcius,

        /// <summary>
        /// HectoPascal.
        /// </summary>
        HectoPascal,

        /// <summary>
        /// Meters.
        /// </summary>
        Meters,

        /// <summary>
        /// Grams per cubicm meter.
        /// </summary>
        GramsPerCubicMeter,

        /// <summary>
        /// Parts per million.
        /// </summary>
        PartsPerMillion,

        /// <summary>
        /// Parts per billion.
        /// </summary>
        PartsPerBillion,

        /// <summary>
        /// Percentage.
        /// </summary>
        Percentage,

        /// <summary>
        /// Millimeters.
        /// </summary>
        Millimeter,
    }
}
