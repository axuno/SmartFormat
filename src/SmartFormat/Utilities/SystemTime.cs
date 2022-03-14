﻿// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// Used for getting DateTime.Now or DateOffset.Now.
    /// Mainly used for unit tests.
    /// </summary>
    public static class SystemTime
    {
        #region : DateTime.Now :

        /// <summary>
        /// Normally this is a pass-through to DateTime.Now, but it can be overridden with SetDateTime( .. ) for unit testing and debugging.
        /// </summary>
        public static Func<DateTime> Now { get; private set; } = () => DateTime.Now;

        /// <summary>
        /// Set time to return when SystemTime.Now() is called.
        /// </summary>
        public static void SetDateTime(DateTime dateTimeNow)
        {
            Now = () => dateTimeNow;
        }

        #endregion

        #region : DateTimeOffset :

        /// <summary>
        /// Normally this is a pass-through to DateTimeOffset.Now, but it can be overridden with SetDateTime( .. ) for unit testing and debugging.
        /// </summary>
        public static Func<DateTimeOffset> OffsetNow { get; private set; } = () => DateTimeOffset.Now;

        /// <summary>
        /// Set time to return when SystemTime.OffsetNow() is called.
        /// </summary>
        public static void SetDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            OffsetNow = () => dateTimeOffset;
        }

        #endregion

        /// <summary>
        /// Resets SystemTime.Now() to return DateTime.Now.
        /// </summary>
        public static void ResetDateTime()
        {
            Now = () => DateTime.Now;
            OffsetNow = () => DateTimeOffset.Now;
        }
    }
}
