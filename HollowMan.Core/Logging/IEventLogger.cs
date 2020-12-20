// <copyright file="IEventLogger.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Logging
{
    using HollowMan.Core.SensorData;

    /// <summary>
    /// Logger interface.
    /// </summary>
    public interface IEventLogger
    {
        /// <summary>
        /// Log an information event.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="arguments">The arguments to pass in.</param>
        void LogInformation(string message, params object[] arguments);

        /// <summary>
        /// Log an information event.
        /// </summary>
        /// <param name="sample">The weather sample to log.</param>
        void LogWeatherObservation(ISensorSample sample);

        /// <summary>
        /// Log a system level start message.
        /// </summary>
        /// <param name="message">The name of the subsystem.</param>
        void LogSystemStart(string message);

        /// <summary>
        /// Log a system level success message.
        /// </summary>
        /// <param name="message">The name of the subsystem.</param>
        void LogSystemSuccess(string message);

        /// <summary>
        /// Log a system error message.
        /// </summary>
        /// <param name="message">The name of the subsystem.</param>
        void LogSystemError(string message);

        /// <summary>
        /// Log a system warning message.
        /// </summary>
        /// <param name="message">The name of the subsystem.</param>
        void LogSystemWarning(string message);

        /// <summary>
        /// Log a sensor start message.
        /// </summary>
        /// <param name="sensorName">The sensor name.</param>
        void LogSensorStart(string sensorName);

        /// <summary>
        /// Log a sensor success message.
        /// </summary>
        /// <param name="sensorName">The sensor name.</param>
        void LogSensorSuccess(string sensorName);

        /// <summary>
        /// Log a sensor error message.
        /// </summary>
        /// <param name="message">The name of the subsystem.</param>
        /// <param name="sensorName">The sensor name.</param>
        void LogSensorError(string message, string sensorName);

        /// <summary>
        /// Log a sensor warning message.
        /// </summary>
        /// <param name="message">The name of the subsystem.</param>
        /// <param name="sensorName">The sensor name.</param>
        void LogSensorWarning(string message, string sensorName);

        /// <summary>
        /// Log a sensor message.
        /// </summary>
        /// <param name="message">The name of the subsystem.</param>
        /// <param name="sensorName">The sensor name.</param>
        void LogSensorMessage(string message, string sensorName);
    }
}