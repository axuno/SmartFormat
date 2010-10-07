using System;



namespace StringFormatEx.Plugins.Core
{
    public class ExtendSourceEventArgs : EventArgs
    {
        private ExtendSourceEventArgs() {}

        public ExtendSourceEventArgs(ICustomSourceInfo sourceInfo)
        {
            SourceInfo = sourceInfo;
        }

        public ICustomSourceInfo SourceInfo { get; private set; }
    }


}