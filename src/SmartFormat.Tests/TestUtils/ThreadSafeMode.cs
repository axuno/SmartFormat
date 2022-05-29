//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Settings;
using SmartFormat.Pooling;

namespace SmartFormat.Tests.TestUtils
{
    /// <summary>
    /// Change <see cref="SmartSettings.IsThreadSafeMode"/> during unit tests.
    /// </summary>
    public static class ThreadSafeMode
    {
        /// <summary>
        /// Sets <see cref="SmartSettings.IsThreadSafeMode"/> to <see langword="true"/>
        /// and resets <see cref="PoolRegistry.Items"/> using the new mode.
        /// </summary>
        /// <returns>The current value of <see cref="SmartSettings.IsThreadSafeMode"/> before switching the mode.</returns>
        public static bool SwitchOn()
        {
            return SwitchTo(true);
        }

        /// <summary>
        /// Sets <see cref="SmartSettings.IsThreadSafeMode"/> to <see langword="true"/>
        /// and resets <see cref="PoolRegistry.Items"/> using the new mode.
        /// </summary>
        /// <returns>The current value of <see cref="SmartSettings.IsThreadSafeMode"/> before switching the mode.</returns>
        public static bool SwitchOff()
        {
            return SwitchTo(false);
        }

        /// <summary>
        /// Sets <see cref="SmartSettings.IsThreadSafeMode"/> to the <paramref name="newSetting"/>.
        /// and resets <see cref="PoolRegistry.Items"/> using the new mode.
        /// </summary>
        /// <returns>The value of <see cref="SmartSettings.IsThreadSafeMode"/> before switching the mode.</returns>
        public static bool SwitchTo(bool newSetting)
        {
            var currentMode = SmartSettings.IsThreadSafeMode;
            SmartSettings.IsThreadSafeMode = newSetting;

            ResetThreadPools(newSetting);

            return currentMode;
        }

        private static void ResetThreadPools(bool mode)
        {
            // Thread pools might not be created in thread-safe mode,
            // so we have to reset them
            foreach (dynamic p in PoolRegistry.Items.Values)
            {
                p.Reset(mode);
            }
        }
    }
}
