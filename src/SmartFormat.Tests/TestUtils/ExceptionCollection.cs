using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

    /// <summary>
    /// Some IEnumerable extensions that catch exceptions into an ExceptionCollection.
    /// Very useful for unit tests and for deferring exceptions.
    /// </summary>
    [DebuggerNonUserCode] // (Steps over all methods)
    public static class ExceptionCollectionExtensions
    {
        /// <summary>
        /// Performs the action for each item, catching all errors into an ExceptionCollection.
        /// Only catches the type of Exception that you specify.
        /// </summary>
        public static ExceptionCollection TryAll<TSource, TException>(this IEnumerable<TSource> sources, Action<TSource> action) where TException : Exception
        {
            var errors = new ExceptionCollection();
            foreach (var source in sources)
            {
                try
                {
                    action(source);
                }
                catch (TException ex)
                {
                    errors.Add(ex);
                }
            }
            return errors;
        }

        /// <summary>
        /// Performs the action for each item, catching all exceptions into an ExceptionCollection.
        /// </summary>
        public static ExceptionCollection TryAll<TSource>(this IEnumerable<TSource> sources, Action<TSource> action)
        {
            var errors = new ExceptionCollection();
            foreach (var source in sources)
            {
                try
                {
                    action(source);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
            return errors;
        }
    }
}
