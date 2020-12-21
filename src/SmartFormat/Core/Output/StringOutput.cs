//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.Text;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Wraps a StringBuilder so it can be used for output.
    /// This is used for the default output.
    /// </summary>
    public class StringOutput : IOutput
    {
        private readonly StringBuilder output;

        public StringOutput()
        {
            output = new StringBuilder();
        }

        public StringOutput(int capacity)
        {
            output = new StringBuilder(capacity);
        }

        public StringOutput(StringBuilder output)
        {
            this.output = output;
        }


        public void Write(string text, IFormattingInfo formattingInfo)
        {
            output.Append(text);
        }

        public void Write(string text, int startIndex, int length, IFormattingInfo formattingInfo)
        {
            output.Append(text, startIndex, length);
        }


        /// <summary>
        /// Returns the results of the StringBuilder.
        /// </summary>
        public override string ToString()
        {
            return output.ToString();
        }
    }
}