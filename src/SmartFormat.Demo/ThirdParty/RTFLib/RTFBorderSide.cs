


 

namespace RTF
{
    using System;

    [Flags]
    public enum RTFBorderSide
    {
        None = 0,
        Left = 0x01,
        Right = 0x02,
        Top = 0x04,
        Bottom = 0x08,
        Default = 0x0F,
        DoubleThickness = 0x10,
        DoubleBorder = 0x20
    }
}


