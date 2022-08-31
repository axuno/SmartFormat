using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Cysharp.Text
{
    internal interface IResettableBufferWriter<T> : IBufferWriter<T>
    {
        void Reset();
    }
}
