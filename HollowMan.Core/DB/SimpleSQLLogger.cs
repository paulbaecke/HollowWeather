// <copyright file="SimpleSQLLogger.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.DB
{
    using System;
    using System.Data.SqlClient;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSQLLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public SimpleSQLLogger(IEventLogger logger)
        {
            this.connection = string.Format("<add your connection>");
            this.insert = "INSERT INTO TestObservation (ObservationTime, Temperature1, Temperature2, Humidity, Pressure, CO2, TVOC, WindGust, WindAverage) VALUES (@now, @temp, @temp2, @hum, @pres, @co2, @tvoc, @windgust, @windavg);";
            this.sqlConnection = new SqlConnection(this.connection);
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Log(IWeatherObservation observation)
        {
            try
            {
                this.logger.LogSystemStart("Simple SQL log");
                var cmd = new SqlCommand(this.insert, this.sqlConnection);
                cmd.Parameters.AddWithValue("@now", observation.TimeStamp);
                cmd.Parameters.AddWithValue("@temp", observation.Temperature1);
                cmd.Parameters.AddWithValue("@temp2", observation.Temperature2);
                cmd.Parameters.AddWithValue("@hum", observation.RelativeHumidity);
                cmd.Parameters.AddWithValue("@pres", observation.BarometricPressure);
                cmd.Parameters.AddWithValue("@co2", observation.CO2);
                cmd.Parameters.AddWithValue("@tvoc", observation.TVOC);
                cmd.Parameters.AddWithValue("@windgust", observation.WindGust);
                cmd.Parameters.AddWithValue("@windavg", observation.WindAverage);
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
