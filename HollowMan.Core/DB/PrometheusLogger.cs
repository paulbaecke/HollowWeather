// <copyright file="PrometheusLogger.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.DB
{
    using System;
    using System.Net.Http;
    using System.Text;
    using HollowMan.Core.Logging;

    /// <summary>
    /// Prometheus logger for weather.
    /// </summary>
    public class PrometheusLogger : IWeatherLogger
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly IEventLogger logger;
        private readonly string targeturl;
        private readonly string[] knownMetrics = new string[]
            {
                "Temperature1",
                "Temperature2",
                "UvA",
                "UvB",
                "UvI",
                "TVOC",
                "CO2",
                "Pressure",
                "BarometricPressure",
                "SealevelPressure",
                "OverIcePressure",
                "OverWaterPressure",
                "RelativeHumidity",
                "AbsoluteHumidity",
                "VapourPressure",
                "CalculatedAltitude",
                "ActualAltitude",
                "HeatIndex",
                "DewPoint",
                "WindGust",
                "WindAverage",
                "WindDirection",
                "Precipitation",
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="PrometheusLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="instancename">the name of the current instance.</param>
        /// <param name="hostname">The Prometheus hostname.</param>
        /// <param name="port">The port to use.</param>
        /// <param name="url">The URL on the server to use.</param>
        public PrometheusLogger(IEventLogger logger, string instancename, string hostname, int port = 9091, string url = "metrics")
        {
            this.logger = logger;
            this.targeturl = $"{hostname}:{port}/{url}/job/weather/instance/{instancename}";
        }

        /// <inheritdoc/>
        public void Log(IWeatherObservation observation)
        {
            this.logger.LogSystemStart($"Prometheus logging: {this.knownMetrics.Length} measures.");
            StringBuilder body = new StringBuilder();
            body.Append($"weather_timestamp {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\n");
            foreach (var label in this.knownMetrics)
            {
                // this.weatherObservation[label].Set(GetValue(label, observation));
                body.Append($"weather_{label.ToLower()} {GetValue(label, observation):F4}\n");
            }

            string tosend = body.ToString();
            var response = Client.PostAsync(this.targeturl, new StringContent(tosend));
            var content = response.Result.Content.ReadAsStringAsync().Result;
            this.logger.LogSystemSuccess($"Prometheus logging: done.");
        }

        private static double GetValue(string label, IWeatherObservation observation)
        {
            switch (label)
            {
                case "Temperature1":
                    return observation.Temperature1;
                case "Temperature2":
                    return observation.Temperature2;
                case "UvA":
                    return observation.UvA;
                case "UvB":
                    return observation.UvB;
                case "UvI":
                    return observation.UvI;
                case "TVOC":
                    return observation.TVOC;
                case "CO2":
                    return observation.CO2;
                case "Pressure":
                    return observation.Pressure;
                case "BarometricPressure":
                    return observation.BarometricPressure;
                case "SealevelPressure":
                    return observation.SealevelPressure;
                case "OverIcePressure":
                    return observation.OverIcePressure;
                case "OverWaterPressure":
                    return observation.OverWaterPressure;
                case "RelativeHumidity":
                    return observation.RelativeHumidity;
                case "AbsoluteHumidity":
                    return observation.AbsoluteHumidity;
                case "VapourPressure":
                    return observation.VapourPressure;
                case "CalculatedAltitude":
                    return observation.CalculatedAltitude;
                case "ActualAltitude":
                    return observation.ActualAltitude;
                case "HeatIndex":
                    return observation.HeatIndex;
                case "DewPoint":
                    return observation.DewPoint;
                case "WindGust":
                    return observation.WindGust;
                case "WindAverage":
                    return observation.WindAverage;
                case "WindDirection":
                    return observation.WindDirection;
                case "Precipitation":
                    return observation.Precipitation;
                default:
                    break;
            }

            return -1;
        }

        private void LogError(System.Exception obj)
        {
            this.logger.LogSystemError($"Prometheus:{obj}");
        }
    }
}
