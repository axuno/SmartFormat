


 

namespace RTF
{
    using System;

    [Flags]
    public enum RTFAlignment
    {
        None = 0,

        /// <summary>Content is vertically aligned at the bottom, and horizontally aligned at the center.</summary>
        /// <filterpriority>1</filterpriority>
        BottomCenter = 512,
        /// <summary>Content is vertically aligned at the bottom, and horizontally aligned on the left.</summary>
        /// <filterpriority>1</filterpriority>
        BottomLeft = 256,
        /// <summary>Content is vertically aligned at the bottom, and horizontally aligned on the right.</summary>
        /// <filterpriority>1</filterpriority>
        BottomRight = 1024,
        /// <summary>Content is vertically aligned in the middle, and horizontally aligned at the center.</summary>
        /// <filterpriority>1</filterpriority>
        MiddleCenter = 32,
        /// <summary>Content is vertically aligned in the middle, and horizontally aligned on the left.</summary>
        /// <filterpriority>1</filterpriority>
        MiddleLeft = 16,
        /// <summary>Content is vertically aligned in the middle, and horizontally aligned on the right.</summary>
        /// <filterpriority>1</filterpriority>
        MiddleRight = 64,
        /// <summary>Content is vertically aligned at the top, and horizontally aligned at the center.</summary>
        /// <filterpriority>1</filterpriority>
        TopCenter = 2,
        /// <summary>Content is vertically aligned at the top, and horizontally aligned on the left.</summary>
        /// <filterpriority>1</filterpriority>
        TopLeft = 1,
        /// <summary>Content is vertically aligned at the top, and horizontally aligned on the right.</summary>
        /// <filterpriority>1</filterpriority>
        TopRight = 4
    }
}


