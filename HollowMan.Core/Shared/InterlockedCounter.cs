// <copyright file="InterlockedCounter.cs" company="Paul Baecke">
// Copyright (c) Paul Baecke. All rights reserved.
// </copyright>

namespace HollowMan.Core.Shared
{
    using System.Threading;

    /// <summary>
    /// Interlocked counter.
    /// </summary>
    public sealed class InterlockedCounter
    {
        // use a meaningful name, 'i' by convention should only be used in a for loop.
        private int current = 0;

        /// <summary>
        /// Increment the counter.
        /// </summary>
        /// <returns>The incremented count.</returns>
        // update the method name to imply that it returns something.
        public int Increment()
        {
            // prefix fields with 'this'
            return Interlocked.Increment(ref this.current);
        }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <returns>The current value.</returns>
        public int GetValue()
        {
            return Interlocked.CompareExchange(ref this.current, 0, 0);
        }

        /// <summary>
        /// Read and reset.
        /// </summary>
        /// <returns>The current value.</returns>
        public int GetAndReset()
        {
            var result = this.GetValue();
            this.Reset();
            return result;
        }

        /// <summary>
        /// Set counter to zero.
        /// </summary>
        public void Reset()
        {
            this.current = 0;
        }
    }
}
