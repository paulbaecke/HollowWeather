// <copyright file="DS18B20.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System.Device.I2c;
    using System.IO;
    using System.Linq;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;

    /// <summary>
    /// DS18B20 Temperature probe driver.
    /// This is GPIO based.
    /// </summary>
    public class DS18B20 : Sensor
    {
        private string pathForW1;

        /// <summary>
        /// Initializes a new instance of the <see cref="DS18B20"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        public DS18B20(SensorController controller)
        {
            this.SensorName = "DS18B20_HMDriver";
            this.Controller = controller;
        }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();

            // find device
            foreach (var directory in Directory.EnumerateDirectories("/sys/bus/w1/devices"))
            {
                if (directory.Split('/').Last().StartsWith("28"))
                {
                    this.pathForW1 = Path.Combine(directory, "w1_slave");
                    continue;
                }
            }

            this.LogStartSuccess();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var result = new SensorSample(this.SensorName);

            var lines = File.ReadAllLines(this.pathForW1);
            int i = 0;
            bool success = false;

            while (!success && i++ < 3)
            {
                success = lines[0].Contains("YES");
                if (success)
                {
                    var rawTemp = lines[1].Split('=').Last();
                    var temp = float.Parse(rawTemp) / 1000.0f;
                    result.AddFinalObservation(temp, "TEMPERATURE2", ObservationUnits.DegreesCelcius);
                    result.AddDiagnostic(lines[0]);
                    result.AddDiagnostic(lines[1]);
                    observation.Temperature2 = temp;
                }
            }

            this.LogTakeReadingComplete();
            return result;
        }
    }
}
