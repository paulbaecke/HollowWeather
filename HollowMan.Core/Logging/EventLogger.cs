// <copyright file="EventLogger.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Logging
{
    using System;
    using HollowMan.Core.SensorData;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Wrapper around logger so we don't expose every class to logger extension.
    /// </summary>
    public class EventLogger : IEventLogger
    {
        private const string SUCCESS = "SUC";
        private const string START = "BEG";
        private const string WARNING = "WRN";
        private const string ERROR = "ERR";
        private const string MESSAGE = "MSG";
        private const string OBSERVATION = "OBS";
        private const string DIAGNOSTICS = "DGN";
        private const string SYSTEM = "SYS";

        private const string SYSTEMFORMAT = "{0}\t{1}\t{2}";
        private const string SENSORFORMAT = "{0}\tSEN\t{1}: {2}";

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public EventLogger(ILogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void LogInformation(string message, params object[] arguments)
        {
            this.logger.LogInformation(message, arguments);
        }

        /// <inheritdoc/>
        public void LogWeatherObservation(ISensorSample sample)
        {
            foreach (var diag in sample.Diagnostics)
            {
                this.logger.LogInformation(SENSORFORMAT, DIAGNOSTICS, sample.SensorName, diag.ToString());
            }

            foreach (var observation in sample.Observations)
            {
                this.logger.LogInformation(SENSORFORMAT, OBSERVATION, sample.SensorName, observation.ToString());
            }
        }

        /// <inheritdoc/>
        public void LogSystemStart(string message)
        {
            this.logger.LogInformation(SYSTEMFORMAT, START, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSystemSuccess(string message)
        {
            this.logger.LogInformation(SYSTEMFORMAT, SUCCESS, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSystemError(string message)
        {
            this.logger.LogCritical(SYSTEMFORMAT, ERROR, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSystemWarning(string message)
        {
            this.logger.LogWarning(SYSTEMFORMAT, WARNING, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSensorStart(string sensorName)
        {
            this.logger.LogInformation(SENSORFORMAT, START, sensorName, $"Starting {sensorName}.");
        }

        /// <inheritdoc/>
        public void LogSensorSuccess(string sensorName)
        {
            this.logger.LogInformation(SENSORFORMAT, SUCCESS, sensorName, $"Successfully started {sensorName}.");
        }

        /// <inheritdoc/>
        public void LogSensorError(string message, string sensorName)
        {
            this.logger.LogError(SENSORFORMAT, ERROR, sensorName, message);
        }

        /// <inheritdoc/>
        public void LogSensorWarning(string message, string sensorName)
        {
            this.logger.LogWarning(SENSORFORMAT, WARNING, sensorName, message);
        }

        /// <inheritdoc/>
        public void LogSensorMessage(string message, string sensorName)
        {
            this.logger.LogInformation(SENSORFORMAT, MESSAGE, sensorName, message);
        }
    }
}
