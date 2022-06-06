//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace SmartFormat.Utilities;

internal static class Validation
{
    private static readonly char[] Valid = new[] { '|', ',', '~' };

    public static char GetValidSplitCharOrThrow(char toCheck)
    {
        return toCheck == Valid[0] || toCheck == Valid[1] || toCheck == Valid[2]
            ? toCheck
            : throw new ArgumentException($"Only '{Valid[0]}', '{Valid[1]}' and '{Valid[2]}' are valid split chars.");
    }
}