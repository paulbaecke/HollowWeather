// <copyright file="VEML6075Async.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Sensors.Drivers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Driver for VEML6075 (async).
    /// </summary>
    public class VEML6075Async : IDisposable
    {
        // I2C Slave Address
        private const byte VEML6075I2CADDRESS = 0x10;

        // Registers
        private const byte VEML6075UVCONF = 0x00;
        private const byte VEML6075UVADATA = 0x07;
        private const byte VEML6075DUMMY = 0x08;
        private const byte VEML6075UVBDATA = 0x09;
        private const byte VEML6075UVCOMP1DATA = 0x0A;
        private const byte VEML6075UVCOMP2DATA = 0x0B;

        // private const byte VEML6075ID = 0x0C;

        // Default Values
        private readonly double uvaacoef = 2.22; // UVA VIS Coefficient
        private readonly double uvabcoef = 1.33; // VA IR Coefficient
        private readonly double uvbccoef = 2.95; // UVB VIS Coefficient
        private readonly double uvbdcoef = 1.74; // UVB IR Coefficient
        private readonly double uvaresp = 0.001461; // UVA Responsivity
        private readonly double uvbresp = 0.002591; // UVB Responsivity

        // Correction Factors
        private readonly double k1 = 0;
        private readonly double k2 = 0;

        private readonly Sensor rootsensor;

        // System
        private bool isInitialized = false;
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VEML6075Async"/> class.
        /// </summary>
        /// <param name="sensor">the sensor to use.</param>
        public VEML6075Async(Sensor sensor)
        {
            // Initiate the sensor.
            this.rootsensor = sensor;
            this.Initialize();
        }

        /// <summary>
        /// From spec doc.
        /// </summary>
        public enum IntegrationTime : byte
        {
            /// <summary>
            /// Timer 50ms
            /// </summary>
            IT050ms = 0b00000000,

            /// <summary>
            /// Timer 100ms
            /// </summary>
            IT100ms = 0b00010000,

            /// <summary>
            /// Timer 200ms
            /// </summary>
            IT200ms = 0b00100000,

            /// <summary>
            /// Timer 400ms
            /// </summary>
            IT400ms = 0b00110000,

            /// <summary>
            /// Timer 800ms
            /// </summary>
            IT800ms = 0b01000000,
        }

        /// <summary>
        /// From spec doc.
        /// </summary>
        public enum DynamicSetting : byte
        {
            /// <summary>
            /// Normal dynamics.
            /// </summary>
            Normal = 0b00000000,

            /// <summary>
            /// High dynamics.
            /// </summary>
            High = 0b00001000,
        }

        /// <summary>
        /// From spec doc.
        /// </summary>
        public enum Trigger : byte
        {
            /// <summary>
            /// Don't force trigger.
            /// </summary>
            NoActiveForceTrigger = 0b00000000,

            /// <summary>
            /// Force trigger.
            /// </summary>
            TriggerOneMeasurement = 0b00000100,
        }

        /// <summary>
        /// From spec doc.
        /// </summary>
        public enum ActiveForceMode : byte
        {
            /// <summary>
            /// Normal mode.
            /// </summary>
            NormalMode = 0b00000000,

            /// <summary>
            /// Active force mode.
            /// </summary>
            ActiveForceMode = 0b00000010,
        }

        /// <summary>
        /// From spec doc.
        /// </summary>
        public enum VEMLPowerMode : byte
        {
            /// <summary>
            /// Power on.
            /// </summary>
            PowerOn = 0b00000000,

            /// <summary>
            /// Shut down please.
            /// </summary>
            ShutDown = 0b00000001,
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Configures the VEML6075 sensor. Verifies if the settings are stored.
        /// </summary>
        /// <param name="uVIT">UV integration time.</param>
        /// <param name="hD">Dynamic setting.</param>
        /// <param name="uVTRIG">Measurement trigger.</param>
        /// <param name="uVAF">Active force mode.</param>
        /// <param name="sD">Power mode.</param>
        /// <returns>True if settings are stored. False if not.</returns>
        public async Task<bool> Config(IntegrationTime uVIT, DynamicSetting hD, Trigger uVTRIG, ActiveForceMode uVAF, VEMLPowerMode sD)
        {
            int tryCounter = 0;

            while (!this.isInitialized)
            {
                await Task.Delay(10).ConfigureAwait(true);
                if (tryCounter++ > 1000)
                {
                    return false;
                }
            }

            try
            {
                byte configCommand = 0x00;

                configCommand += (byte)uVIT;
                configCommand += (byte)hD;
                configCommand += (byte)uVTRIG;
                configCommand += (byte)uVAF;
                configCommand += (byte)sD;

                this.rootsensor.WriteByte(VEML6075UVCONF, configCommand);

                Thread.Sleep(100);

                var writeConfig = this.ReadRegisterTwoBytes(VEML6075UVCONF);
                this.rootsensor.LogMessage($"Config: {writeConfig}");

                if ((configCommand & 0b11111011) == writeConfig)
                {
                    this.rootsensor.LogMessage("VEML Ready");
                    return true;
                }
                else
                {
                    this.rootsensor.LogMessage("VEML Write Failed");
                    return false;
                }
            }
#pragma warning disable CA1031 // This is intentional, want to swallow.
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.rootsensor.LogError(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Reads RAW UVA.
        /// </summary>
        /// <returns>RAW UVA Value.</returns>
        public ushort ReadRAWUVA()
        {
            return this.ReadRegisterTwoBytes(VEML6075UVADATA);
        }

        /// <summary>
        /// Reads RAW UVB.
        /// </summary>
        /// <returns>RAW UVB Value.</returns>
        public ushort ReadRAWUVB()
        {
            return this.ReadRegisterTwoBytes(VEML6075UVBDATA);
        }

        /// <summary>
        /// Reads RAW UVD.
        /// </summary>
        /// <returns>RAW UVD Value.</returns>
        public ushort ReadRAWUVD()
        {
            return this.ReadRegisterTwoBytes(VEML6075DUMMY);
        }

        /// <summary>
        /// Reads Noise Compensation Channel 1 data which allows only visible noise to pass through.
        /// </summary>
        /// <returns>UV Comp 1 Value.</returns>
        public ushort ReadRAWUVCOMP1()
        {
            return this.ReadRegisterTwoBytes(VEML6075UVCOMP1DATA);
        }

        /// <summary>
        /// Reads Noise Compensation Channel 2 data which allows only infrared noise to pass through.
        /// </summary>
        /// <returns>UV Comp 2 Value.</returns>
        public ushort ReadRAWUVCOMP2()
        {
            return this.ReadRegisterTwoBytes(VEML6075UVCOMP2DATA);
        }

        /// <summary>
        /// Calculates Compensated UVA.
        /// </summary>
        /// <returns>UVA Comp Value.</returns>
        public double CalculateCompensatedUVA()
        {
            // Formula:
            // UVAcalc = UVA - a x UVcomp1 - b x UVcomp2
            ushort uva = this.ReadRegisterTwoBytes(VEML6075UVADATA);
            ushort uvcomp1 = this.ReadRegisterTwoBytes(VEML6075UVCOMP1DATA);
            ushort uvcomp2 = this.ReadRegisterTwoBytes(VEML6075UVCOMP2DATA);
            double uVAcalc = 1.0 * (uva - (this.uvaacoef * uvcomp1) - (this.uvabcoef * uvcomp2));
            return uVAcalc;
        }

        /// <summary>
        /// Calculates Compensated UVB.
        /// </summary>
        /// <returns>UVB Comp Value.</returns>
        public double CalculateCompensatedUVB()
        {
            // Formula:
            // UVBcalc = UVB - c x UVcomp1 - d x UVcomp2
            ushort uvb = this.ReadRegisterTwoBytes(VEML6075UVBDATA);
            ushort uvcomp1 = this.ReadRegisterTwoBytes(VEML6075UVCOMP1DATA);
            ushort uvcomp2 = this.ReadRegisterTwoBytes(VEML6075UVCOMP2DATA);
            double uVBcalc = 1.0 * (uvb - (this.uvbccoef * uvcomp1) - (this.uvbdcoef * uvcomp2));
            return uVBcalc;
        }

        /// <summary>
        /// Calculates the UV Index A.
        /// </summary>
        /// <returns>UV Index A Value.</returns>
        public double CalculateUVIndexA()
        {
            // Formula:
            // UVIA = UVAcalc x k1 x UVAresponsivity
            ushort uva = this.ReadRegisterTwoBytes(VEML6075UVADATA);
            ushort uvcomp1 = this.ReadRegisterTwoBytes(VEML6075UVCOMP1DATA);
            ushort uvcomp2 = this.ReadRegisterTwoBytes(VEML6075UVCOMP2DATA);
            double uVAcalc = 1.0 * (uva - (this.uvaacoef * uvcomp1) - (this.uvabcoef * uvcomp2));
            double uVIA = uVAcalc * this.k1 * this.uvaresp;
            return uVIA;
        }

        /// <summary>
        /// Calculates the UV Index B.
        /// </summary>
        /// <returns>UV Index B Value.</returns>
        public double CalculateUVIndexB()
        {
            // Formula:
            // UVIB = UVBcalc x k2 x UVBresponsivity
            ushort uvb = this.ReadRegisterTwoBytes(VEML6075UVBDATA);
            ushort uvcomp1 = this.ReadRegisterTwoBytes(VEML6075UVCOMP1DATA);
            ushort uvcomp2 = this.ReadRegisterTwoBytes(VEML6075UVCOMP2DATA);
            double uVBcalc = 1.0 * (uvb - (this.uvbccoef * uvcomp1) - (this.uvbdcoef * uvcomp2));
            double uVIB = uVBcalc * this.k2 * this.uvbresp;
            return uVIB;
        }

        /// <summary>
        /// Calculates the Average UV Index.
        /// </summary>
        /// <returns>Average UV Index Value.</returns>
        public double CalculateAverageUVIndex()
        {
            // Formula:
            // UVAcomp = (UVA - UVD) - a * (UVcomp1 - UVD) - b * (UVcomp2 - UVD);
            // UVBcomp = (UVB - UVD) - c * (UVcomp1 - UVD) - d * (UVcomp2 - UVD);
            // UVI = ((UVBcomp * UVBresp) + (UVAcomp * UVAresp)) / 2;
            ushort uva = this.ReadRegisterTwoBytes(VEML6075UVADATA);
            ushort uvb = this.ReadRegisterTwoBytes(VEML6075UVBDATA);
            ushort uvd = this.ReadRegisterTwoBytes(VEML6075DUMMY);
            ushort uvcomp1 = this.ReadRegisterTwoBytes(VEML6075UVCOMP1DATA);
            ushort uvcomp2 = this.ReadRegisterTwoBytes(VEML6075UVCOMP2DATA);
            double uVAcomp = 1.0 * ((uva - uvd) - (this.uvaacoef * (uvcomp1 - uvd)) - (this.uvabcoef * (uvcomp2 - uvd)));
            double uVBcomp = 1.0 * ((uvb - uvd) - (this.uvbccoef * (uvcomp1 - uvd)) - (this.uvbdcoef * (uvcomp2 - uvd)));
            double uVI = 1.0 * (((uVBcomp * this.uvbresp) + (uVAcomp * this.uvaresp)) / 2);

            if (uVI < 0)
            {
                uVI = 0;
            }

            return uVI;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Are we disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.isInitialized = false;
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Initiates the sensor.
        /// </summary>
        private void Initialize()
        {
            this.isInitialized = true;
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private ushort ReadRegisterTwoBytes(byte reg)
        {
            ushort value;
            byte[] readBuffer = this.rootsensor.ReadRawBytes(reg, 2);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (ushort)(h + l);

            return value;
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private byte[] ReadRegisterTwoBytesArray(byte reg)
        {
            return this.rootsensor.ReadRawBytes(reg, 2);
        }
    }
}