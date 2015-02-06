using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SmartFormat.Tests.Common
{
	// Note: I just found out that .NET 4 has an exception called "AggregateException" that sounds very similar to this ExceptionCollection.

	/// <summary>
	/// An exception that represents a collection of exceptions of a specific type.
	/// Useful for deferring the throwing of exceptions.
	/// </summary>
	[DebuggerNonUserCode] // (Steps over all methods)
	public class ExceptionCollection<TException> : Exception, IList<TException> where TException : Exception
	{
		#region: Private Fields :

		protected readonly List<TException> innerExceptions = new List<TException>();

		#endregion

		#region: Constructors :

		public ExceptionCollection() { }
		public ExceptionCollection(TException exception)
		{
			this.innerExceptions.Add(exception);
		}
		public ExceptionCollection(IEnumerable<TException> exceptions)
		{
			this.innerExceptions.AddRange(exceptions);
		}

		#endregion

		#region: Public Methods :

		/// <summary>
		/// This method will throw this exception if it contains any inner exceptions.
		/// </summary>
		public void ThrowIfNotEmpty()
		{
			if (this.innerExceptions.Count > 0)
			{
				throw this;
			}
		}

		/// <summary>
		/// Adds a range of exceptions to the exception collection.
		/// </summary>
		/// <param name="exceptions"></param>
		public void AddRange(IEnumerable<TException> exceptions)
		{
			this.innerExceptions.AddRange(exceptions);
		}

		/// <summary>
		/// Executes an action, and if an exception is thrown, adds it to the collection.
		/// </summary>
		/// <param name="action"></param>
		public void Try(Action action)
		{
			try
			{
				action();
			}
			catch (TException ex)
			{
				this.Add(ex);
			}
		}

		#endregion

		#region: IList Implementation :

		public void Add(TException exception)
		{
			this.innerExceptions.Add(exception);
		}

		public void Clear()
		{
			this.innerExceptions.Clear();
		}

		public bool Contains(TException exception)
		{
			return this.innerExceptions.Contains(exception);
		}

		public void CopyTo(TException[] array, int arrayIndex)
		{
			this.innerExceptions.CopyTo(array, arrayIndex);
		}

		public bool Remove(TException exception)
		{
			return this.innerExceptions.Remove(exception);
		}

		public int Count
		{
			get
			{
				return this.innerExceptions.Count;
			}
		}
		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerator<TException> GetEnumerator()
		{
			return this.innerExceptions.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int IndexOf(TException exception)
		{
			return this.innerExceptions.IndexOf(exception);
		}

		public void Insert(int index, TException exception)
		{
			this.innerExceptions.Insert(index, exception);
		}

		public void RemoveAt(int index)
		{
			this.innerExceptions.RemoveAt(index);
		}

		public TException this[int index]
		{
			get
			{
				return this.innerExceptions[index];
			}
			set
			{
				this.innerExceptions[index] = value;
			}
		}

		#endregion

		#region: Exception Overrides :

		public override string Message
		{
			get
			{
				return string.Format(
									 "{0} {1}{2} thrown:\n{3}",
									 innerExceptions.Count,
									 typeof(TException).Name,
									 innerExceptions.Count == 1 ? " was" : "s were",
									 string.Join("\n",
										 innerExceptions
										 .Select((e,i) => string.Format("\t#{0}: {1}",
																		i+1,
																		"\t" + e.Message)).ToArray())
									 );
			}
		}

		#endregion
	}

	/// <summary>
	/// An exception that represents a collection of exceptions.
	/// Useful for deferring the throwing of exceptions.
	/// </summary>
	[DebuggerNonUserCode] // (Steps over all methods)
	public class ExceptionCollection : ExceptionCollection<Exception>
	{
		public ExceptionCollection() { }
		public ExceptionCollection(Exception exception) : base(exception) { }
		public ExceptionCollection(IEnumerable<Exception> exceptions) : base(exceptions) { }
		public static ExceptionCollection Combine(params ExceptionCollection[] combineAllExceptions)
		{
			var combined = new ExceptionCollection();
			foreach (var exceptionCollection in combineAllExceptions)
			{
				combined.AddRange(exceptionCollection);
			}
			return combined;
		}

		/// <summary>
		/// Adds a new Exception to the ExceptionCollection.
		/// </summary>
		/// <param name="message">The issue message that explains the reason for the exception.
		/// This message can optionally contain string.Format placeholders for the following arguments.</param>
		/// <param name="args">An optional list of objects to use for formatting the message.</param>
		public void AddNewException(string message, params object[] args)
		{
			AddNewException(null, message, args);
		}
		/// <summary>
		/// Adds a new Exception to the ExceptionCollection.
		/// </summary>
		/// <param name="innerException">The exception that is the cause of the current exception.  Can be null.</param>
		/// <param name="message">The issue message that explains the reason for the exception.
		/// This message can optionally contain string.Format placeholders for the following arguments.</param>
		/// <param name="args">An optional list of objects to use for formatting the message.</param>
		public void AddNewException(Exception innerException, string message, params object[] args)
		{
			if (args != null && args.Length > 0)
			{
				message = string.Format(message, args);
			}
			this.Add(new Exception(message, innerException));
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
		public static ExceptionCollection<TException> TryAll<TSource, TException>(this IEnumerable<TSource> sources, Action<TSource> action) where TException : Exception
		{
			var errors = new ExceptionCollection<TException>();
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
