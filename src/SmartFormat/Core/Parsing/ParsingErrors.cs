// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Represents parsing errors in a format string.
/// This exception only gets thrown when Parser.ErrorAction is set to ThrowError.
/// </summary>
[Serializable]
public class ParsingErrors : Exception   //NOSONAR
{
    private Format _result = InitializationObject.Format;

    #region: Create, initialize, return to pool :

    /// <summary>
    /// CTOR for object pooling.
    /// Immediately after creating the instance, an overload of 'Initialize' must be called.
    /// </summary>
    public ParsingErrors()
    {
        // Inserted for clarity and documentation
    }

    /// <summary>
    /// Initializes the instance of <see cref="ParsingErrors"/>.
    /// </summary>
    /// <param name="result">The <see cref="Format"/> that caused the error.</param>
    /// <returns>This <see cref="ParsingErrors"/> instance.</returns>
    public ParsingErrors Initialize(Format result)
    {
        _result = result;
        return this;
    }

    /// <summary>
    /// Clears the <see cref="Issues"/> list.
    /// <para>This method gets called by <see cref="ParsingErrorsPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.</para>
    /// </summary>
    public void Clear()
    {
        Issues.Clear();
    }

    #endregion

    /// <summary>
    /// Gets a <see cref="IList{T}"/> of <see cref="ParsingIssue"/>s./>
    /// </summary>
    public List<ParsingIssue> Issues { get; } = new();

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="IList{T}"/> of <see cref="ParsingIssue"/>s contains elements.
    /// </summary>
    public bool HasIssues => Issues.Count > 0;

    /// <summary>
    /// Gets the short version of an error message.
    /// </summary>
    public string MessageShort =>
        $"The format string has {Issues.Count} issue{(Issues.Count == 1 ? string.Empty : "s")}: {string.Join(", ", Issues.Select(i => i.Issue).ToArray())}";

    /// <summary>
    /// Gets the long version of an error message.
    /// </summary>
    public override string Message
    {
        get
        {
            var arrows = new StringBuilder();
            var lastArrow = 0;
            foreach (var issue in Issues)
            {
                arrows.Append(new string('-', issue.Index - lastArrow));
                if (issue.Length > 0)
                {
                    arrows.Append(new string('^', Math.Max(issue.Length, 1)));
                    lastArrow = issue.Index + issue.Length;
                }
                else
                {
                    arrows.Append('^');
                    lastArrow = issue.Index + 1;
                }
            }

            return
                $"The format string has {Issues.Count} issue{(Issues.Count == 1 ? string.Empty : "s")}:\n{string.Join(", ", Issues.Select(i => i.Issue).ToArray())}\nIn: \"{_result.BaseString}\"\nAt:  {arrows} ";
        }
    }

    ///<inheritdoc/>
    [Obsolete("This API supports obsolete formatter-based serialization. It will be removed in version 4.")]
    protected ParsingErrors(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    /// Adds a new <see cref="ParsingIssue"/>.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="issue"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    public void AddIssue(Format parent, string issue, int startIndex, int endIndex)
    {
        Issues.Add(new ParsingIssue(issue, startIndex, endIndex - startIndex));
    }

    /// <summary>
    /// The class represents a list of parsing issues.
    /// </summary>
    public class ParsingIssue
    {
        /// <summary>
        /// Creates a new instance of <see cref="ParsingIssue"/>.
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        public ParsingIssue(string issue, int index, int length)
        {
            Issue = issue;
            Index = index;
            Length = length;
        }

        /// <summary>
        /// Gets the index within the format string, where an error occurred.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the length starting from the <see cref="Index"/> which has errors.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the description of an error issue.
        /// </summary>
        public string Issue { get; }
    }
}
