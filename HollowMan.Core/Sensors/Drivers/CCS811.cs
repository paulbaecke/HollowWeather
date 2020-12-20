// <copyright file="CCS811.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Collections.Generic;
    using System.Device.I2c;
    using System.Threading;
    using HollowMan.Core.SensorControllers;
    using HollowMan.Core.SensorData;
    using HollowMan.Core.Shared;

    /// <summary>
    /// CCS811 driver.
    /// </summary>
    public class CCS811 : Sensor
    {
#pragma warning disable IDE0051 // Unused members reserved for future use
        private const byte DEVICEID = 0x5B;
        private const byte CCS811STATUS = 0x00;
        private const byte CCS811MEASMODE = 0x01;
        private const byte CCS811MEAS1S = 0x10;
        private const byte CCS811ALGRESULTDATA = 0x02;

        // private const byte CCS811_RAW_DATA = 0x03;
        //  private const byte CCS811_ENV_DATA = 0x05;
        //  private const byte CCS811_NTC = 0x06;
        //  private const byte CCS811_THRESHOLDS = 0x10;
        private const byte CCS811BASELINE = 0x11;
        private const byte CCS811HWID = 0x20;

        // private const byte CCS811_HW_VERSION = 0x21;
        //   private const byte CCS811_FW_BOOT_VERSION = 0x23;
        //   private const byte CCS811_FW_APP_VERSION = 0x24;
        private const byte CCS811ERRORID = 0xE0;
        private const byte CCS811APPSTART = 0xF4;
        private const byte CCS811SWRESET = 0xFF;
        private const byte CCS811ID = 0x81;
#pragma warning restore IDE0051 // Unused members reserved for future use

        private readonly int warmupCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CCS811"/> class.
        /// </summary>
        /// <param name="controller">The sensor controller.</param>
        /// <param name="warmupCount">number of warmup reading to do.</param>
        public CCS811(SensorController controller, int warmupCount)
        {
            this.DeviceId = DEVICEID;
            this.Messages = new List<string>();
            this.warmupCount = warmupCount;
            this.SensorName = "CCS811_HMDriver";
            this.Controller = controller;
        }

        /// <summary>
        /// Gets the list of messages.
        /// </summary>
        public IList<string> Messages { get; private set; }

        /// <inheritdoc/>
        public override string GetDiagnostics()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.LogStartInitialization();
            this.Device = this.Controller.GetI2CConnection(this.DeviceId);
            var resetCommand = new byte[] { CCS811SWRESET, 0x11, 0xE5, 0x72, 0x8A };
            this.Device.Write(resetCommand);

            Thread.Sleep(100);

            byte hwId = (byte)ByteOperations.GetUChar(this.ReadRawByte(CCS811HWID));
            if (hwId != CCS811ID)
            {
                throw new System.Exception(string.Format("Incorrect ID, expected {0}, got {1}", CCS811HWID, hwId));
            }

            // check internal status
            this.CheckError("Init");

            // app valid
            var status = this.ReadRawByte(CCS811STATUS);
            this.LogMessage(string.Format("Status afer INIT OK? {0}", status == 16));
            if ((status & 1) << 4 != 0)
            {
                this.CheckError("App not valid");
            }

            this.Device.WriteByte(CCS811APPSTART);
            Thread.Sleep(100);
            this.CheckError("Start");

            this.WriteByte(CCS811MEASMODE, CCS811MEAS1S);
            this.CheckError("MEAS");

            Thread.Sleep(1500);
            var bytes = this.ReadRawBytes(CCS811BASELINE, 2);
            this.LogMessage(string.Format("Baseline: {0}", (bytes[0] << 8) | bytes[1]));

            // data availabe?
            this.LogMessage(string.Format("Data available: {0}", !((this.ReadRawByte(CCS811STATUS) & 1) << 3 == 0)));
            this.CheckError("READ");

            this.LogMessage("Warm up");
            for (int i = 0; i < this.warmupCount; i++)
            {
                var data = this.ReadRawBytes(CCS811ALGRESULTDATA, 4);
                this.LogMessage($"RawBytes: {ByteOperations.PrintByteArray(data)}");
                Thread.Sleep(5000);
            }

            this.CheckError("WARMUP");
            this.LogMessage("CCS811 Ready");
            this.LogStartSuccess();
        }

        /// <inheritdoc/>
        public override ISensorSample TakeReading(WeatherObservation observation)
        {
            this.LogTakeReadingStart();
            var sensorResult = new SensorSample(this.SensorName);
            var data = this.ReadRawBytes(CCS811ALGRESULTDATA, 4);

            sensorResult.AddDiagnostic(ByteOperations.PrintByteArray(data));

            var co2 = (data[0] << 8) | data[1];
            var tvoc = (data[2] << 8) | data[3];
            sensorResult.AddFinalObservation(co2, "CO2", ObservationUnits.PartsPerMillion);
            sensorResult.AddFinalObservation(tvoc, "TVOC", ObservationUnits.PartsPerBillion);
            observation.CO2 = co2;
            observation.TVOC = tvoc;

            this.LogTakeReadingComplete();

            return sensorResult;
        }

        private void CheckError(string mode, int expected = 16)
        {
            var errorCode = this.ReadRawByte(CCS811STATUS);
            if ((errorCode & 1) << 0 == 1)
            {
                this.LogError(string.Format("{2} {0}: {1}", errorCode, this.GetError(), mode));
            }

            this.LogMessage(string.Format("Status {2} afer {1} OK? {0}", errorCode == expected, mode, errorCode));
        }

        private string GetError()
        {
            var errorCode = this.ReadRawByte(CCS811ERRORID);
            string errorMessage = "Unknown";

            if ((errorCode & 1 << 5) == 1)
            {
                errorMessage = "HeaterSupply";
            }
            else if ((errorCode & 1 << 4) == 1)
            {
                errorMessage = "HeaterFault";
            }
            else if ((errorCode & 1 << 3) == 1)
            {
                errorMessage = "MaxResistance";
            }
            else if ((errorCode & 1 << 2) == 1)
            {
                errorMessage = "MeasModeInvalid";
            }
            else if ((errorCode & 1 << 1) == 1)
            {
                errorMessage = "ReadRegInvalid";
            }
            else if ((errorCode & 1 << 0) == 1)
            {
                errorMessage = "MsgInvalid";
            }

            return string.Format("ERROR: {0} [{1}]", errorMessage, errorCode);
        }
    }
}
