// <copyright file="Sensor.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors
{
    using System;
    using System.Buffers.Binary;
    using System.Device.I2c;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;

    /// <summary>
    /// Base class for sensors.
    /// </summary>
    public abstract class Sensor : ISensor
    {
        private const byte READMASK = 0x80;

        /// <inheritdoc/>
        public byte DeviceId { get; protected set; }

        /// <inheritdoc/>
        public I2cDevice Device { get; protected set; }

        /// <inheritdoc/>
        public SensorController Controller { get; protected set; }

        /// <inheritdoc/>
        public bool IsInitialized { get; protected set; }

        /// <inheritdoc/>
        public string SensorName { get; protected set; }

        /// <summary>
        /// Conver a byte buffer to an int.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <returns>An int.</returns>
        public static int ConvertToInt24LittleEndian(ReadOnlySpan<byte> buffer)
        {
            byte mostSignificantByte = buffer[2];
            Span<byte> b = stackalloc byte[4]
            {
                buffer[0],
                buffer[1],
                mostSignificantByte,
                (mostSignificantByte >> 7) != 0 ? (byte)0xff : (byte)0x00,
            };

            return BinaryPrimitives.ReadInt32LittleEndian(b);
        }

        /// <summary>
        /// Write a byte to the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="data">The data to write.</param>
        public void WriteByte(SensorRegister register, byte data)
        {
            this.WriteByte((byte)register, data);
        }

        /// <summary>
        /// Write a byte to the specified register.
        /// </summary>
        /// <param name="register">The register id.</param>
        /// <param name="data">The data to write.</param>
        public void WriteByte(byte register, byte data)
        {
            Span<byte> buff = stackalloc byte[2]
            {
                register,
                data,
            };

            this.Device.Write(buff);
        }

        /// <summary>
        /// Write data to the specified register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="data">The byte array of data to write.</param>
        public void WriteBytes(byte register, byte[] data)
        {
            Span<byte> buffer = stackalloc byte[1 + data.Length];
            buffer[0] = register;
            for (int i = 1; i < data.Length; i++)
            {
                buffer[i] = data[i - 1];
            }

            this.Device.Write(buffer);
        }

        /// <summary>
        /// Read int from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns>An int.</returns>
        public int ReadInt24(SensorRegister register)
        {
            Span<byte> val = stackalloc byte[3];
            this.Read(register, val);
            return ConvertToInt24LittleEndian(val);
        }

        /// <summary>
        /// Read int from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns>An int.</returns>
        public int ReadInt24(byte register)
        {
            Span<byte> val = stackalloc byte[3];
            this.Read(register, val);
            return ConvertToInt24LittleEndian(val);
        }

        /// <summary>
        /// Read int from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns>An int.</returns>
        public short ReadInt16(SensorRegister register)
        {
            Span<byte> val = stackalloc byte[2];
            this.Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
        }

        /// <summary>
        /// Read int from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns>An int.</returns>
        public short ReadInt16(byte register)
        {
            Span<byte> val = stackalloc byte[2];
            this.Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
        }

        /// <summary>
        /// Read int from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="buffer">the buffer to read.</param>
        public void Read(SensorRegister register, Span<byte> buffer)
        {
            this.Read((byte)register, buffer);
        }

        /// <summary>
        /// Read int from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="buffer">the buffer to read.</param>
        public void Read(byte register, Span<byte> buffer)
        {
            this.Device.WriteByte((byte)(register | READMASK));
            this.Device.Read(buffer);
        }

        /// <summary>
        /// Read raw from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="buffer">the buffer to read.</param>
        public void ReadRaw(byte register, Span<byte> buffer)
        {
            this.Device.WriteByte(register);
            this.Device.Read(buffer);
        }

        /// <summary>
        /// Read raw from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns>A single byte.</returns>
        public byte Read(SensorRegister register)
        {
            return this.Read((byte)register);
        }

        /// <summary>
        /// Read raw from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="length">the buffer lenth to read.</param>
        /// <returns>A byte array.</returns>
        public byte[] ReadBytes(byte register, int length)
        {
            Span<byte> val = stackalloc byte[length];
            this.Read(register, val);
            return val.ToArray();
        }

        /// <summary>
        /// Read raw from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns>A single byte.</returns>
        public byte Read(byte register)
        {
            this.Device.WriteByte((byte)(register | READMASK));
            return this.Device.ReadByte();
        }

        /// <summary>
        /// Read raw from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <returns>A single byte.</returns>
        public byte ReadRawByte(byte register)
        {
            this.Device.WriteByte(register);
            return this.Device.ReadByte();
        }

        /// <summary>
        /// Read raw from register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="length">The length of the buffer.</param>
        /// <returns>A byte array.</returns>
        public byte[] ReadRawBytes(byte register, int length)
        {
            Span<byte> val = stackalloc byte[length];
            this.ReadRaw(register, val);
            return val.ToArray();
        }

        /// <summary>
        /// Log start initialization.
        /// </summary>
        public void LogStartInitialization()
        {
            this.Controller.Logger.LogSensorStart(this.SensorName);
        }

        /// <summary>
        /// Log start success.
        /// </summary>
        public void LogStartSuccess()
        {
            this.Controller.Logger.LogSensorSuccess(this.SensorName);
        }

        /// <summary>
        /// Log error.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogError(string message)
        {
            this.Controller.Logger.LogSensorError(message, this.SensorName);
        }

        /// <summary>
        /// Log warning.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogWarning(string message)
        {
            this.Controller.Logger.LogSensorWarning(message, this.SensorName);
        }

        /// <summary>
        /// Log warning.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogMessage(string message)
        {
            this.Controller.Logger.LogSensorMessage(message, this.SensorName);
        }

        /// <summary>
        /// Log reading success.
        /// </summary>
        public void LogTakeReadingComplete()
        {
            this.Controller.Logger.LogSensorMessage("Complete reading", this.SensorName);
        }

        /// <summary>
        /// Log start reading.
        /// </summary>
        public void LogTakeReadingStart()
        {
            this.Controller.Logger.LogSensorMessage("Start reading", this.SensorName);
        }

        /// <inheritdoc/>
        public abstract string GetDiagnostics();

        /// <inheritdoc/>
        public abstract void Initialize();

        /// <inheritdoc/>
        public abstract ISensorSample TakeReading(WeatherObservation observation);
    }
}
