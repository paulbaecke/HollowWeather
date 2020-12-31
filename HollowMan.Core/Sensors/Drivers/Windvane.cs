// <copyright file="Windvane.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Collections.Generic;
    using System.Device.Spi;
    using System.Text;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;
    using HollowMan.Core.Shared;
    using Iot.Device.Adc;

    /// <summary>
    /// A switch based MCP3008 windvane implementation.
    /// </summary>
    public class Windvane : Sensor
    {
        private static readonly Dictionary<double, WindDirection> WindData = new Dictionary<double, WindDirection>(16)
        {
             { 0.4, new WindDirection("N", 0.0, 33000) },
             { 1.4, new WindDirection("NNE", 22.5, 6570) },
             { 1.2, new WindDirection("NE", 45.0, 8200) },
             { 2.8, new WindDirection("ENE", 67.5, 891) },
             { 2.7, new WindDirection("E", 90.0, 1000) },
             { 2.9, new WindDirection("ESE", 112.5, 688) },
             { 2.2, new WindDirection("SE", 135.0, 2200) },
             { 2.5, new WindDirection("SSE", 157.5, 1410) },
             { 1.8, new WindDirection("S", 180.0, 3900) },
             { 2.0, new WindDirection("SSW", 202.5, 3140) },
             { 0.7, new WindDirection("SW", 225.0, 16000) },
             { 0.8, new WindDirection("WSW", 247.5, 14120) },
             { 0.1, new WindDirection("W", 270.0, 120000) },
             { 0.3, new WindDirection("WNW", 292.5, 42120) },
             { 0.2, new WindDirection("NW", 315.0, 64900) },
             { 0.6, new WindDirection("NNW", 337.5, 21880) },
        };

        private readonly SpiDevice spiDevice;
        private readonly SpiConnectionSettings spiConnectionSettings;
        private readonly int busId;
        private readonly double vIn;
        private readonly int vDivide;

        private Mcp3008 adc;

        /// <summary>
        /// Initializes a new instance of the <see cref="Windvane"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        /// <param name="busId">Bus Id of the MCP3008 chip.</param>
        /// <param name="vIn">The input voltage. For Pi, this is 3.3.</param>
        /// <param name="vDivide">Voltage divider resistor in ohms.</param>
        public Windvane(SensorController controller, int busId, double vIn, int vDivide)
        {
            this.Controller = controller;
            this.spiConnectionSettings = new SpiConnectionSettings(busId);
            this.spiDevice = SpiDevice.Create(this.spiConnectionSettings);
            this.SensorName = "MCP3800_Windvane";
            this.busId = busId;
            this.vIn = vIn;
            this.vDivide = vDivide;
        }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            var result = new StringBuilder();
            if (!this.IsInitialized)
            {
                throw new System.Exception("Must initialize device");
            }

            result.AppendLine($"Bus Id: {this.busId}");
            result.AppendLine($"vIn: {this.vIn}");
            result.AppendLine($"vDivide: {this.vDivide}");
            return result.ToString();
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();
            this.adc = new Mcp3008(this.spiDevice);
            this.IsInitialized = true;
            this.LogStartSuccess();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var result = new SensorSample(this.SensorName);
            double currentState = this.adc.Read(0) / 1024.0d;
            double correctedVoltage = this.CorrectVoltage(currentState);
            var windDirection = this.GetDirection(correctedVoltage);
            if (windDirection != null)
            {
                this.LogMessage($"adc raw: {currentState}");
                this.LogMessage($"adc corrected reading: {correctedVoltage}");
                this.LogMessage($"wind direction: {windDirection.Direction} ({windDirection.Degrees})");
                result.AddFinalObservation(windDirection.Degrees, "WINDDIRECTION", ObservationUnits.Default);
                observation.WindDirection = windDirection.Degrees;
                this.LogTakeReadingComplete();
            }
            else
            {
                this.LogError("Windvane reading error");
            }

            return result;
        }

        private WindDirection GetDirection(double correctedVoltage)
        {
            if (WindData.TryGetValue(correctedVoltage, out WindDirection direction))
            {
                return direction;
            }

            return null;
        }

        private double CorrectVoltage(double adcReading)
        {
            var correctedVoltage = Math.Round(adcReading * this.vIn, 1);

            if (correctedVoltage == 0.5)
            {
                correctedVoltage = 0.4;
            }

            if (correctedVoltage == 1.3)
            {
                correctedVoltage = 1.2;
            }

            if (correctedVoltage == 1.9)
            {
                correctedVoltage = 1.8;
            }

            if (correctedVoltage == 2.6)
            {
                correctedVoltage = 2.7;
            }

            if (correctedVoltage == 1.9)
            {
                correctedVoltage = 1.8;
            }

            if (correctedVoltage == 2.3)
            {
                correctedVoltage = 2.2;
            }

            if (correctedVoltage == 1.3)
            {
                correctedVoltage = 1.2;
            }

            return correctedVoltage;
        }
    }
}
