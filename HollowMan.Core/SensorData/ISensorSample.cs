// <copyright file="ISensorSample.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>
// <author>Paul Baecke</author>
namespace HollowMan.Core.SensorData
{
    using System.Collections.Generic;

    /// <summary>
    /// ISensorSample is the core interface for reporting results
    /// Each sensor can collect a series of observations on each poll
    /// Some sensors need to be invoked by the sensor controller to take a reading,
    /// others are interupt driven.
    /// This is abstracted from the reporting components.
    /// </summary>
    public interface ISensorSample
    {
        /// <summary>
        /// Gets the diagnostics. Each reading/polling event may return additional diagnostic messages.
        /// </summary>
        IList<string> Diagnostics { get; }

        /// <summary>
        /// Gets the list of observations for the current reporting period/sample.
        /// </summary>
        IList<IObservation> Observations { get; }

        /// <summary>
        /// Gets the sensor name.
        /// </summary>
        string SensorName { get; }

        /// <summary>
        /// Add a diagnostic to the sample.
        /// </summary>
        /// <param name="message">The message string, should contain any debugging data/state messages from the underlying sensor.</param>
        void AddDiagnostic(string message);

        /// <summary>
        /// Add a final value that may be interesting to downstream reports.
        /// </summary>
        /// <param name="value">
        /// Double representation of the metric. double is chosen as none of the underlying sensors available have
        /// enough precision to warrant double.
        /// </param>
        /// <param name="name">The name of the observation.</param>
        /// <param name="units">The units for the observation.</param>
        void AddFinalObservation(double value, string name, ObservationUnits units);

        /// <summary>
        /// Add intermediate state, which is likely only to be interesting for debugging purposes.
        /// </summary>
        /// <param name="value">
        /// Double representation of the metric. double is chosen as none of the underlying sensors available have
        /// enough precision to warrant double.
        /// </param>
        /// <param name="name">The name of the observation.</param>
        /// <param name="units">The units for the observation.</param>
        void AddIntermediateObservation(double value, string name, ObservationUnits units);
    }
}