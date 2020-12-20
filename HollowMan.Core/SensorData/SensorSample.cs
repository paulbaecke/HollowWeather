// <copyright file="SensorSample.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>
// <author>Paul Baecke</author>
namespace HollowMan.Core.SensorData
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A sensor sample result.
    /// </summary>
    public class SensorSample : ISensorSample
    {
        /// <summary>
        /// The time the samples were taken.
        /// </summary>
        private readonly DateTime sampleTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorSample"/> class.
        /// </summary>
        /// <param name="sensorName">The name of the sensor.</param>
        public SensorSample(string sensorName)
        {
            this.Observations = new List<IObservation>();
            this.sampleTime = DateTime.Now;
            this.Diagnostics = new List<string>();
            this.SensorName = sensorName;
        }

        /// <inheritdoc/>
        public IList<IObservation> Observations { get; private set; }

        /// <inheritdoc/>
        public IList<string> Diagnostics { get; private set; }

        /// <inheritdoc/>
        public string SensorName { get; private set; }

        /// <inheritdoc/>
        public void AddDiagnostic(string message)
        {
            this.Diagnostics.Add(message);
        }

        /// <inheritdoc/>
        public void AddIntermediateObservation(double value, string name, ObservationUnits units)
        {
            this.Observations.Add(new Observation(value, this.sampleTime, name, units, ObservationType.RAW));
        }

        /// <inheritdoc/>
        public void AddFinalObservation(double value, string name, ObservationUnits units)
        {
            this.Observations.Add(new Observation(value, this.sampleTime, name, units, ObservationType.CALIBRATED));
        }
    }
}
