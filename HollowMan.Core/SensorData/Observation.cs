// <copyright file="Observation.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.SensorData
{
    using System;
    using HollowMan.Core.Shared;

    /// <summary>
    /// Represents an observation.
    /// </summary>
    public class Observation : IObservation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Observation"/> class.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <param name="when">The sample timestamp.</param>
        /// <param name="name">The name for the observation.</param>
        /// <param name="units">The units the observation is in.</param>
        /// <param name="observationType">The type of the observation.</param>
        public Observation(double value, DateTime when, string name, ObservationUnits units, ObservationType observationType)
        {
            this.Value = value;
            this.Observed = when;
            this.Name = name;
            this.ObservationType = observationType;
            this.Units = units;
        }

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public DateTime Observed { get; private set; }

        /// <inheritdoc/>
        public ObservationType ObservationType { get; private set; }

        /// <inheritdoc/>
        public SensorType SensorType { get; private set; }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public ObservationUnits Units { get; private set; }

        /// <summary>
        /// Helper class to human-friendly render units.
        /// </summary>
        /// <param name="unit">The observation unit.</param>
        /// <returns>The human friendly string. Or empty string.</returns>
        public static string GetStringFromUnit(ObservationUnits unit)
        {
            switch (unit)
            {
                case ObservationUnits.Default:
                    return string.Empty;
                case ObservationUnits.KmPerHour:
                    return "km/h";
                case ObservationUnits.DegreesCelcius:
                    return "\u00B0C";
                case ObservationUnits.GramsPerCubicMeter:
                    return "g/m3";
                case ObservationUnits.HectoPascal:
                    return "hPa";
                case ObservationUnits.PartsPerBillion:
                    return "ppb";
                case ObservationUnits.PartsPerMillion:
                    return "ppm";
                case ObservationUnits.Percentage:
                    return "%";
                case ObservationUnits.Millimeter:
                    return "mm";
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format($"{this.Name}: {this.Value} {GetStringFromUnit(this.Units)}");
        }
    }
}
