using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Text;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// Extensions to <see cref="ZString"/>.
    /// </summary>
    public static class ZStringExtensions
    {
        // DefaultBufferSize of Utf16ValueStringBuilder
        internal const int DefaultBufferSize = 32768;

        /// <summary>
        /// Calculates the estimated output string capacity for a <see cref="Format"/>.
        /// </summary>
        /// <param name="format"></param>
        /// <returns>The estimated output string capacity for a <see cref="Format"/>.</returns>
        internal static int CalcCapacity(Format format)
        {
            return format.Length + format.Items.Count * 8;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Utf16ValueStringBuilder"/> with the given initial capacity.
        /// </summary>
        /// <param name="format">The estimated buffer capacity will be calculated from the <see cref="Format"/> instance.</param>
        internal static Utf16ValueStringBuilder CreateStringBuilder(Format format)
        {
            var sb = new Utf16ValueStringBuilder(false);
            return CreateStringBuilder(CalcCapacity(format));
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="Utf16ValueStringBuilder"/> with the given initial capacity.
        /// </summary>
        /// <param name="capacity">The estimated capacity required. This will reduce or avoid incremental buffer increases.</param>
        internal static Utf16ValueStringBuilder CreateStringBuilder(int capacity)
        {
            var sb = new Utf16ValueStringBuilder(false);
            if(capacity > DefaultBufferSize)
                sb.Grow(capacity - DefaultBufferSize);

            return sb;
        }
    }
}
