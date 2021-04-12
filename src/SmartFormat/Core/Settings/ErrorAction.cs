//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// Determines how format errors are handled.
    /// </summary>
    public enum ErrorAction
    {
        /// <summary>Throws an exception.  This is only recommended for debugging, so that formatting errors can be easily found.</summary>
        ThrowError,

        /// <summary>Includes an issue message in the output</summary>
        OutputErrorInResult,

        /// <summary>Ignores errors and tries to output the data anyway</summary>
        Ignore,

        /// <summary>Leaves invalid tokens unmodified in the text.</summary>
        MaintainTokens
    }
}