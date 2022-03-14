using System;
using System.Collections.ObjectModel;

namespace SmartFormat.Tests.TestUtils
{
    public class ExceptionCollection : AggregateException
    {
        private readonly Collection<Exception> _innerExceptions = new();

        /// <summary>
        /// This method will throw this exception if it contains any inner exceptions.
        /// </summary>
        public void ThrowIfNotEmpty()
        {
            if (InnerExceptions.Count > 0 || _innerExceptions.Count > 0)
            {
                throw this;
            }
        }

        public void Add(Exception exception)
        {
            _innerExceptions.Add(exception);
        }
    }
}
