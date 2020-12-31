// <copyright file="WeatherDaemon.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowWeather.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HollowMan.Core;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Weather daemon class.
    /// </summary>
    public class WeatherDaemon : BackgroundService, IDisposable
    {
        private readonly ILogger logger;
        private readonly IOptions<WeatherConfig> config;
        private readonly WeatherManager weatherManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherDaemon"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public WeatherDaemon(ILogger<WeatherDaemon> logger, IOptions<WeatherConfig> config)
        {
            this.logger = logger;
            this.config = config;
            this.weatherManager = new WeatherManager(this.logger, 60, 85, -5.02d);
        }

        /// <summary>
        /// Start the daemon.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use.</param>
        /// <returns>A task.</returns>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting daemon: " + config.Value.DaemonName);
            this.weatherManager.StartObserving();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop the daemon.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use.</param>
        /// <returns>A task.</returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.weatherManager.Stop();
            logger.LogInformation("Stopping daemon.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose the daemon.
        /// </summary>
        public override void Dispose()
        {
            logger.LogInformation("Disposing....");
            this.weatherManager.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return this.StartAsync(stoppingToken);
        }
    }

}
