using System;



namespace StringFormatEx.Plugins.Core
{
    public class ExtendFormatEventArgs : EventArgs
    {
        private ExtendFormatEventArgs() {}

        public ExtendFormatEventArgs(CustomFormatInfo formatInfo)
        {
            FormatInfo = formatInfo;
        }

        public CustomFormatInfo FormatInfo { get; private set; }
    }


}