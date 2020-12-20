// <copyright file="IObservation.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>
// <author>Paul Baecke</author>
namespace HollowMan.Core.SensorData
{
    using System;
    using HollowMan.Core.Shared;

    /// <summary>
    /// Represents a single observation.
    /// </summary>
    public interface IObservation
    {
        /// <summary>
        /// Gets the name to use for the observation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the observation type.
        /// </summary>
        ObservationType ObservationType { get; }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        DateTime Observed { get; }

        /// <summary>
        /// Gets the sensor type used.
        /// </summary>
        SensorType SensorType { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        double Value { get; }

        /// <summary>
        /// Gets the units for the measurement.
        /// </summary>
        ObservationUnits Units { get; }
    }
}