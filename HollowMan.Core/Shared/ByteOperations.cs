// <copyright file="ByteOperations.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Shared
{
    using System;
    using System.Text;

    /// <summary>
    /// This class contains common byte operations.
    /// </summary>
    public static class ByteOperations
    {
        /// <summary>
        /// Get an unsigned short from byte array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="index">The index.</param>
        /// <returns>An unsigned short.</returns>
        public static ushort GetUShort(byte[] bytes, int index)
        {
            return BitConverter.ToUInt16(bytes, index);
        }

        /// <summary>
        /// Get a short from byte array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="index">The index.</param>
        /// <returns>An short.</returns>
        public static int GetShort(byte[] bytes, int index)
        {
            return BitConverter.ToInt16(bytes, index);
        }

        /// <summary>
        /// Get a character from byte array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="index">The index.</param>
        /// <returns>A character.</returns>
        public static char GetChar(byte[] bytes, int index)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var elem = (int)bytes[index];
            if (elem > 127)
            {
                elem -= 256;
            }

            return (char)elem;
        }

        /// <summary>
        /// Get an character from byte.
        /// </summary>
        /// <param name="data">The byte.</param>
        /// <returns>A character.</returns>
        public static char GetChar(byte data)
        {
            var elem = (int)data;
            if (elem > 127)
            {
                elem -= 256;
            }

            return (char)elem;
        }

        /// <summary>
        /// Get an character from byte array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="index">The index.</param>
        /// <returns>A character.</returns>
        public static char GetUChar(byte[] bytes, int index)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (index < 0 || index > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var elem = bytes[index] & 0xFF;
            return (char)elem;
        }

        /// <summary>
        /// Get an character from byte.
        /// </summary>
        /// <param name="data">The byte.</param>
        /// <returns>A character.</returns>
        public static char GetUChar(byte data)
        {
            var elem = data & 0xFF;
            return (char)elem;
        }

        /// <summary>
        /// Prints a byte array as a string for debugging.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <returns>A string for the byte array.</returns>
        public static string PrintByteArray(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0},", b);
            }

            return sb.ToString().Trim(',');
        }
    }
}
