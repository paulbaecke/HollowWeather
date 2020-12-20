// <copyright file="EventLogger.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Logging
{
    using System;
    using HollowMan.Core.SensorData;

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

        private const string SYSTEMFORMAT = "{0}\t{1}\t{2}\t{3}";
        private const string SENSORFORMAT = "{0}\t{1}\tSEN\t{2}: {3}";

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogger"/> class.
        /// </summary>
        public EventLogger()
        {
        }

        /// <inheritdoc/>
        public void LogInformation(string message, params object[] arguments)
        {
            Console.WriteLine(message, arguments);
        }

        /// <inheritdoc/>
        public void LogWeatherObservation(ISensorSample sample)
        {
            var timestamp = Timestamp();
            foreach (var diag in sample.Diagnostics)
            {
                Console.WriteLine(SENSORFORMAT, timestamp, DIAGNOSTICS, sample.SensorName, diag.ToString());
            }

            foreach (var observation in sample.Observations)
            {
                Console.WriteLine(SENSORFORMAT, timestamp, OBSERVATION, sample.SensorName, observation.ToString());
            }
        }

        /// <inheritdoc/>
        public void LogSystemStart(string message)
        {
            Console.WriteLine(SYSTEMFORMAT, Timestamp(), START, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSystemSuccess(string message)
        {
            Console.WriteLine(SYSTEMFORMAT, Timestamp(), SUCCESS, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSystemError(string message)
        {
            Console.WriteLine(SYSTEMFORMAT, Timestamp(), ERROR, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSystemWarning(string message)
        {
            Console.WriteLine(SYSTEMFORMAT, Timestamp(), WARNING, SYSTEM, message);
        }

        /// <inheritdoc/>
        public void LogSensorStart(string sensorName)
        {
            Console.WriteLine(SENSORFORMAT, Timestamp(), START, sensorName, $"Starting {sensorName}.");
        }

        /// <inheritdoc/>
        public void LogSensorSuccess(string sensorName)
        {
            Console.WriteLine(SENSORFORMAT, Timestamp(), SUCCESS, sensorName, $"Successfully started {sensorName}.");
        }

        /// <inheritdoc/>
        public void LogSensorError(string message, string sensorName)
        {
            Console.WriteLine(SENSORFORMAT, Timestamp(), ERROR, sensorName, message);
        }

        /// <inheritdoc/>
        public void LogSensorWarning(string message, string sensorName)
        {
            Console.WriteLine(SENSORFORMAT, Timestamp(), WARNING, sensorName, message);
        }

        /// <inheritdoc/>
        public void LogSensorMessage(string message, string sensorName)
        {
            Console.WriteLine(SENSORFORMAT, Timestamp(), MESSAGE, sensorName, message);
        }

        private static string Timestamp()
        {
            return DateTime.Now.ToString("u");
        }
    }
}
