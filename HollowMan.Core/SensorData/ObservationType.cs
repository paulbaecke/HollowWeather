// <copyright file="ObservationType.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.SensorData
{
    /// <summary>
    /// What kind of observation type is it.
    /// </summary>
    public enum ObservationType
    {
        /// <summary>
        /// Diagnostic.
        /// </summary>
        RAW,

        /// <summary>
        /// Usable for reporting.
        /// </summary>
        CALIBRATED,
    }
}
