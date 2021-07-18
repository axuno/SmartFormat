using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    internal class SourceTypeEqualityComparer : IEqualityComparer<ISource>
    {
        public bool Equals(ISource x, ISource y)
        {
            return x.GetType() == y.GetType();
        }

        public int GetHashCode(ISource obj)
        {
            return obj.GetHashCode();
        }
    }
}
