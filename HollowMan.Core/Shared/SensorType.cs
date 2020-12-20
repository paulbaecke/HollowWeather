// <copyright file="SensorType.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Shared
{
    /// <summary>
    /// What kind of sensor is it.
    /// </summary>
    public enum SensorType
    {
        /// <summary>
        /// Default type.
        /// </summary>
        UNKNOWN,

        /// <summary>
        /// Temperature sensor.
        /// </summary>
        TEMPERATURE,

        /// <summary>
        /// Humidity sensor.
        /// </summary>
        HUMIDITY,

        /// <summary>
        /// Pressure sensor.
        /// </summary>
        PRESSURE,
    }
}
