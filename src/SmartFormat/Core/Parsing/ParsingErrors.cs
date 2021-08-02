//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents parsing errors in a format string.
    /// This exception only gets thrown when Parser.ErrorAction is set to ThrowError.
    /// </summary>
    public class ParsingErrors : Exception
    {
        private readonly Format _result;

        /// <summary>
        /// Creates a new instance of <see cref="ParsingErrors"/>.
        /// </summary>
        /// <param name="result">The <see cref="Format"/> that caused the error.</param>
        public ParsingErrors(Format result)
        {
            _result = result;
            Issues = new List<ParsingIssue>();
        }

        /// <summary>
        /// Gets a <see cref="IList{T}"/> of <see cref="ParsingIssue"/>s./>
        /// </summary>
        public List<ParsingIssue> Issues { get; }

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
                var arrows = string.Empty;
                var lastArrow = 0;
                foreach (var issue in Issues)
                {
                    arrows += new string('-', issue.Index - lastArrow);
                    if (issue.Length > 0)
                    {
                        arrows += new string('^', Math.Max(issue.Length, 1));
                        lastArrow = issue.Index + issue.Length;
                    }
                    else
                    {
                        arrows += '^';
                        lastArrow = issue.Index + 1;
                    }
                }

                return
                    $"The format string has {Issues.Count} issue{(Issues.Count == 1 ? string.Empty : "s")}:\n{string.Join(", ", Issues.Select(i => i.Issue).ToArray())}\nIn: \"{_result.BaseString}\"\nAt:  {arrows} ";
            }
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
}