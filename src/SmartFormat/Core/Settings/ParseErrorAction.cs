// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// Determines how parsing errors are handled.
    /// </summary>
    public enum ParseErrorAction
    {
        /// <summary>
        /// Throws an <see cref="System.Exception"/>, if an error occurs.
        /// This is only recommended for debugging, so that formatting errors can be easily found.
        /// </summary>
        ThrowError,

        /// <summary>
        /// Includes an issue message in the output.
        /// </summary>
        OutputErrorInResult,

        /// <summary>
        /// Ignores errors and tries to output the data anyway.
        /// </summary>
        Ignore,

        /// <summary>
        /// Leaves invalid tokens unmodified in the text.
        /// </summary>
        MaintainTokens
    }
}