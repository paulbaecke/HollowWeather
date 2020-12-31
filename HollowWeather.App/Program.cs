// <copyright file="Program.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>
namespace HollowWeather.App
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Weather daemon
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The app runner.
        /// </summary>
        /// <param name="args">Optional arguments.</param>
        /// <returns>Async task to run.</returns>
        public static void Main(string[] args)
        {
            Console.WriteLine("HollowMan Weather");
            Console.WriteLine("Press ctrl-c or ctrl-break to exit");

            CreateHostBuilder(args).Build().Run();                                    
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<WeatherConfig>(hostContext.Configuration.GetSection("Daemon"));
                    services.AddHostedService<WeatherDaemon>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });
    }
}
