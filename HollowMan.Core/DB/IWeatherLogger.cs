// <copyright file="IWeatherLogger.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.DB
{
    /// <summary>
    /// Log the weather to somewhere permanent.
    /// </summary>
    public interface IWeatherLogger
    {
        /// <summary>
        /// Log an observation.
        /// </summary>
        /// <param name="observation">The observation to log.</param>
        void Log(IWeatherObservation observation);
    }
}