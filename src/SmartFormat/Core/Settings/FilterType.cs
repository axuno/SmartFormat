// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

namespace SmartFormat.Core.Settings;

/// <summary>
/// Determines the filter type for allowed or disallowed characters.
/// </summary>
public enum FilterType    
{
    /// <summary>
    /// Use a list of characters that are allowed. The default characters are<br/>
    /// alphanumeric characters (upper and lower case), plus '_' and '-'.<br/>
    /// </summary>
    Allowlist,

    /// <summary>
    /// All Unicode characters are allowed, except those in the blocklist.
    /// The default blocklist characters are all control characters (ASCII 0-31 and 127).
    /// </summary>
    Blocklist
}
