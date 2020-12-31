// <copyright file="SimpleSQLLogger.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.DB
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using HollowMan.Core.Logging;

    /// <summary>
    /// Logs to SQL.
    /// </summary>
    public class SimpleSQLLogger : IWeatherLogger
    {
        private readonly string connection;
        private readonly string insert;
        private readonly SqlConnection sqlConnection;
        private readonly IEventLogger logger;
        private readonly object syncroot = new object();
        private readonly List<IWeatherObservation> observations = new List<IWeatherObservation>();
        private readonly int publishIntervalInMinutes;
        private DateTime lastPublish = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSQLLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="interval">The interval in minutes to save values at.</param>
        public SimpleSQLLogger(IEventLogger logger, int interval)
        {
            this.connection = string.Format("<connection>");
            this.insert = "INSERT INTO TestObservation (ObservationTime, Temperature1, Temperature2, Humidity, Pressure, CO2, TVOC, WindGust, WindAverage) VALUES (@now, @temp, @temp2, @hum, @pres, @co2, @tvoc, @windgust, @windavg);";
            this.sqlConnection = new SqlConnection(this.connection);
            this.logger = logger;
            this.publishIntervalInMinutes = interval;
        }

        /// <inheritdoc/>
        public void Log(IWeatherObservation observation)
        {
            lock (this.syncroot)
            {
                this.observations.Add(observation);
                if ((DateTime.Now - this.lastPublish).TotalMinutes > this.publishIntervalInMinutes)
                {
                    try
                    {
                        this.logger.LogSystemStart("Simple SQL log");
                        var cmd = new SqlCommand(this.insert, this.sqlConnection);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@temp", this.observations.Average(p => p.Temperature1));
                        cmd.Parameters.AddWithValue("@temp2", this.observations.Average(p => p.Temperature2));
                        cmd.Parameters.AddWithValue("@hum", this.observations.Average(p => p.RelativeHumidity));
                        cmd.Parameters.AddWithValue("@pres", this.observations.Average(p => p.BarometricPressure));
                        cmd.Parameters.AddWithValue("@co2", this.observations.Average(p => p.CO2));
                        cmd.Parameters.AddWithValue("@tvoc", this.observations.Average(p => p.TVOC));
                        cmd.Parameters.AddWithValue("@windgust", this.observations.Max(p => p.WindGust));
                        cmd.Parameters.AddWithValue("@windavg", this.observations.Average(p => p.WindAverage));
                        this.sqlConnection.Open();
                        cmd.ExecuteNonQuery();
                        this.sqlConnection.Close();
                        this.logger.LogSystemSuccess("Simple SQL log");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogSystemError($"Simple SQL log failed {ex}");
                    }
                }
            }
        }
    }
}
