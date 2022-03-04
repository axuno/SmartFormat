//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace SmartFormat.Utilities
{
    internal class Validation
    {
        public static char GetValidSplitCharOrThrow(char toCheck)
        {
            var valid = new[] { '|', ',', '~' };
            return toCheck == valid[0] || toCheck == valid[1] || toCheck == valid[2]
                ? toCheck
                : throw new ArgumentException($"Only '{valid[0]}', '{valid[1]}' and '{valid[2]}' are valid split chars.");
        }
    }
}
