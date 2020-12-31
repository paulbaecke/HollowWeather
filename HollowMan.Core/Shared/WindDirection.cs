// <copyright file="WindDirection.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Shared
{
    /// <summary>
    /// Represents a wind diretion relationship to resistance for the windvance.
    /// </summary>
    public class WindDirection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindDirection"/> class.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="ohms">The reference resistance.</param>
        public WindDirection(string direction, double degrees, int ohms)
        {
            this.Direction = direction;
            this.Degrees = degrees;
            this.Ohms = ohms;
        }

        /// <summary>
        /// Gets the human-friendly direction.
        /// </summary>
        public string Direction { get; private set; }

        /// <summary>
        /// Gets the degrees.
        /// </summary>
        public double Degrees { get; private set; }

        /// <summary>
        /// Gets the reference resistance.
        /// </summary>
        public int Ohms { get; private set; }
    }
}
