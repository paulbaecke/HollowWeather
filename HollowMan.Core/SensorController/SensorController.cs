// <copyright file="SensorController.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>
// <author>Paul Baecke</author>
namespace HollowMan.Core.SensorControllers
{
    using System;
    using System.Collections.Generic;
    using System.Device.Gpio;
    using System.Device.I2c;
    using HollowMan.Core.Logging;

    /// <summary>
    /// Sensor controller calss to manage interop with I2c and GPIO.
    /// </summary>
    public class SensorController : IDisposable
    {
        private readonly IDictionary<byte, I2cDevice> devices;

        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorController" /> class.
        /// </summary>
        /// <param name="logger">The event logger to us.</param>
        public SensorController(IEventLogger logger)
        {
            this.devices = new Dictionary<byte, I2cDevice>();
            this.GPIOController = new GpioController();
            this.Logger = logger;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SensorController"/> class.
        /// </summary>
        ~SensorController()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the GpioController for this device.
        /// </summary>
        public GpioController GPIOController { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public IEventLogger Logger { get; private set; }

        /// <summary>
        /// Add an I2c Device for a byte address.
        /// </summary>
        /// <param name="address">The address to use.</param>
        public void AddDevice(byte address)
        {
            if (!this.devices.ContainsKey(address))
            {
                var connection = CreateI2cDevice(address);
                this.devices.Add(address, connection);
            }
        }

        /// <summary>
        /// Get the I2cDevice for the id.
        /// </summary>
        /// <param name="id">The id of the device.</param>
        /// <returns>The I2c devic, or null.</returns>
        public I2cDevice GetI2CConnection(byte id)
        {
            if (this.devices.TryGetValue(id, out I2cDevice device))
            {
                return device;
            }

            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the underlying devices.
        /// </summary>
        /// <param name="disposing">Are we disposing now.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.GPIOController.Dispose();
                }

                this.disposedValue = true;
            }
        }

        private static I2cDevice CreateI2cDevice(byte address)
        {
            var settings = new I2cConnectionSettings(1, address);
            return I2cDevice.Create(settings);
        }
    }
}
