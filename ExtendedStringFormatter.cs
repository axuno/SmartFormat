using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using StringFormatEx.Plugins.Core;



namespace StringFormatEx
{
    public class ExtendedStringFormatter
    {

        #region Default static instance

        private static ExtendedStringFormatter _default = GetDefault();

        public static ExtendedStringFormatter Default
        {
            get { return _default; }
            set { _default = value; }
        }

        #endregion


        #region Factory Methods

        public static ExtendedStringFormatter GetDefault()
        {
            var formatter = new ExtendedStringFormatter();
            formatter.InvalidSelectorAction = ErrorAction.OutputErrorInResult;
            formatter.InvalidFormatAction = ErrorAction.OutputErrorInResult;
            formatter.AddPlugins(GetDefaultPlugins());

            return formatter;
        }

        public static ExtendedStringFormatter GetDefaultThatThrowsOnErrors()
        {
            var formatter = new ExtendedStringFormatter();
            formatter.InvalidSelectorAction = ErrorAction.ThrowError;
            formatter.InvalidFormatAction = ErrorAction.ThrowError;
            formatter.AddPlugins(GetDefaultPlugins());

            return formatter;
        }

        private static IStringFormatterPlugin[] GetDefaultPlugins()
        {
            return new IStringFormatterPlugin[] {
                                                    new Plugins._DefaultSourcePlugin(),
                                                    new Plugins._DefaultFormatPlugin(),
                                                    new Plugins.ConditionalPlugin(),
                                                    new Plugins.ArrayPlugin()
                                                };
        }

        #endregion



        #region FormatEx overloads

        /// <summary>
        /// Formats the format string using the parameters provided.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public string FormatEx(string format, params object[] args) 
        {
            StringWriter output = new StringWriter(new StringBuilder((format.Length * 2)));
            //  Guessing a length can help performance a little.
            FormatExInternal(new CustomFormatInfo(this, output, format, args));
            return output.ToString();
        }
    

        /// <summary>
        /// Performs the CustomFormat and outputs to a Stream.
        /// </summary>
        public void FormatEx(Stream output, string format, params object[] args) 
        {
            FormatExInternal(new CustomFormatInfo(this, new StreamWriter(output), format, args));
        }
    

        /// <summary>
        /// Performs the CustomFormat and outputs to a TextWriter.
        /// </summary>
        /// <param name="output">Common types of TextWriters are StringWriter and StreamWriter.</param>
        public void FormatEx(TextWriter output, string format, params object[] args) 
        {
            FormatExInternal(new CustomFormatInfo(this, output, format, args));
        }

        #endregion


        #region AddPlugin[s]

        public void AddPlugin(IStringFormatterPlugin plugin)
        {
            foreach (var handler in plugin.GetSourceExtensions()) {
                ExtendSourceEvent += handler;
            }

            foreach (var handler in plugin.GetFormatExtensions()) {
                ExtendFormatEvent += handler;
            }
        }

        public void AddPlugins(IEnumerable<IStringFormatterPlugin> plugins)
        {
            foreach (var stringFormatterPlugin in plugins) {
                AddPlugin(stringFormatterPlugin);
            }
        }

        #endregion


        #region Error Actions

        public ErrorAction InvalidSelectorAction { get; set; }
        public ErrorAction InvalidFormatAction { get; set; }

        #endregion



        #region ExtendSourceEvent event

        private IDictionary<CustomFormatPriorities, IList<EventHandler<ExtendSourceEventArgs>>> _customSourceHandlers;

        private void OnExtendSourceEvent(ExtendSourceEventArgs e)
        {
            if (_customSourceHandlers != null) {
                foreach (var list in _customSourceHandlers.Values) {
                    foreach (var handler in list) {
                        if (e.SourceInfo.Handled) {
                            return;
                        }
                        handler.Invoke(this, e);
                    }
                }
            }
        }

        protected event EventHandler<ExtendSourceEventArgs> ExtendSourceEvent {
            add
            {
                if (_customSourceHandlers == null) {
                    _customSourceHandlers = new SortedDictionary<CustomFormatPriorities, IList<EventHandler<ExtendSourceEventArgs>>>();
                }

                //  Let's search for the "CustomFormatPriorityAttribute" to see if we should add this handler at a higher priority in the handler list:
                CustomFormatPriorities handlerPriority = CustomFormatPriorities.Normal;

                //  default priority
                foreach (CustomFormatPriorityAttribute pa in value.Method.GetCustomAttributes(typeof(CustomFormatPriorityAttribute), true)) {
                    handlerPriority = pa.Priority;
                    //  There should never be more than 1 PriorityAttribute
                }

                //  Make sure there is a list for this priority:
                if (!_customSourceHandlers.ContainsKey(handlerPriority)) {
                    _customSourceHandlers.Add(handlerPriority, new List<EventHandler<ExtendSourceEventArgs>>());
                }

                //  Add the new handler to the list:
                _customSourceHandlers[handlerPriority].Add(value);
            }

            remove
            {
                if (_customSourceHandlers != null) {
                    foreach (var list in _customSourceHandlers.Values) {
                        if (list.Remove(value)) {
                            return;
                        }
                    }
                }
            }
        }

        #endregion


        #region ExtendFormatEvent

        private IDictionary<CustomFormatPriorities, IList<EventHandler<ExtendFormatEventArgs>>> _customFormatHandlers;

        private void OnExtendFormatEvent(ExtendFormatEventArgs e)
        {
            if (_customFormatHandlers != null) {
                foreach (var list in _customFormatHandlers.Values) {
                    foreach (var handler in list) {
                        if (e.FormatInfo.Handled) {
                            return;
                        }
                        handler.Invoke(this, e);
                    }
                }
            }
        }



        /// <summary>
        /// An event that allows custom formatting to occur.
        /// 
        /// Why is it Custom?  2 reasons:
        /// " This event short-circuits if a handler sets the output, to improve efficiency and redundancy
        /// " This event allows you to set an "event priority" by applying the CustomFormatPriorityAttribute to the handler method!  (See "How to add your own Custom Formatter:" below)
        /// 
        /// This adds a little complexity to the event, but there are occassions when we need the CustomFormat handlers to execute in a certain order.
        /// </summary>
        private event EventHandler<ExtendFormatEventArgs> ExtendFormatEvent
        {
            add
            {
                if (_customFormatHandlers == null) {
                    //  Initialize the event dictionary:
                    _customFormatHandlers = new SortedDictionary<CustomFormatPriorities, IList<EventHandler<ExtendFormatEventArgs>>>();
                }

                //  Let's search for the "CustomFormatPriorityAttribute" to see if we should add this handler at a higher priority in the handler list:
                CustomFormatPriorities handlerPriority = CustomFormatPriorities.Normal; //  default priority

                foreach (CustomFormatPriorityAttribute pa in value.Method.GetCustomAttributes(typeof(CustomFormatPriorityAttribute), true)) {
                    handlerPriority = pa.Priority;
                    //  There should never be more than 1 PriorityAttribute
                }

                //  Make sure there is a list for this priority:
                if (!_customFormatHandlers.ContainsKey(handlerPriority)) {
                    _customFormatHandlers.Add(handlerPriority, new List<EventHandler<ExtendFormatEventArgs>>());
                }

                //  Add the new handler to the list:
                _customFormatHandlers[handlerPriority].Add(value);
            }

            remove
            {
                if (_customFormatHandlers != null) {
                    foreach (var list in _customFormatHandlers.Values) {
                        if (list.Remove(value)) {
                            return;
                        }
                    }
                }
            }
        }

        #endregion

 

        #region OnInvalidSelector

        /// <summary>
        /// Determines what to do when an Invalid Selector is found.
        /// 
        /// Returns True if we should just continue; False if we should skip this item.
        /// </summary>
        private bool OnInvalidSelector(string format, CustomFormatInfo info, PlaceholderInfo placeholder)
        {
            string invalidSelector = format.Substring(placeholder.selectorStart, placeholder.selectorLength);

            string message;
            switch (InvalidSelectorAction) {
                case ErrorAction.ThrowError:
                    //  Let's give a detailed description of the error:
                    message = FormatEx(
                            ("Invalid Format String.\\n" +
                             ("Could not evaluate \"{0}\": \"{1}\" is not a member of {2}.\\n" +
                              ("The error occurs at position {3} of the following format string:\\n" + "{4}"))),
                            invalidSelector, info.Selector, info.CurrentType, placeholder.placeholderStart, format);
                    throw new ArgumentException(message, invalidSelector);
                case ErrorAction.OutputErrorInResult:
                    //  Let's put the placeholder back,
                    //  along with the error.
                    //  Example: {Person.Name.ABC}  becomes  {Person.Name.ABC:(Error: "ABC" is not a member of String)}
                    message = ("{" + (FormatEx("{0}:(Error: \"{1}\" is not a member of {2})", invalidSelector,
                                                    info.Selector, info.CurrentType) + "}"));
                    info.WriteError(message, placeholder);
                    return false;
                case ErrorAction.Ignore:
                    //  Allow formatting to continue!
                    break;
            }
            return true;
        }

        #endregion


        #region OnInvalidFormat

        /// <summary>
        /// Determines what to do when an Invalid Selector is found.
        /// </summary>
        private void OnInvalidFormat(string format, CustomFormatInfo info, PlaceholderInfo placeholder, Exception ex)
        {
            string selector = format.Substring(placeholder.selectorStart, placeholder.selectorLength);
            string invalidFormat = format.Substring(placeholder.formatStart, placeholder.formatLength);
            string errorMessage = ex.Message;

            if (ex.GetType() is FormatException) {
                errorMessage = FormatEx("\"{0}\" is not a valid format specifier for {1}", invalidFormat,
                                            info.CurrentType);
            }

            string message;
            switch (InvalidFormatAction) {
                case ErrorAction.ThrowError:
                    //  Let's give a detailed description of the error:
                    message = FormatEx(
                            ("Invalid Format String.\\n" +
                             ("Could not evaluate {{0}} because {1}.\\n" +
                              ("The error occurs at position {2} of the following format string:\\n" + "{3}"))), selector,
                            errorMessage, placeholder.placeholderStart, format);
                    throw new ArgumentException(message, invalidFormat, ex);
                case ErrorAction.OutputErrorInResult:
                    //  Let's put the placeholder back,
                    //  along with the error.
                    //  Example: {Person.Birthday:x}  becomes  {Person.Birthday:(Error: "x" is an invalid format specifier)}
                    message = ("{" + (FormatEx("{0}:(Error: {1})", selector, errorMessage) + "}"));
                    info.WriteError(message, placeholder);
                    break;
                case ErrorAction.Ignore:
                    //  Allow formatting to continue!
                    break;
            }
        }

        #endregion
   

    
        #region FormatExInternal
    
        /// <summary>
        /// Does the actual work.
        /// </summary>
        internal void FormatExInternal(CustomFormatInfo info) 
        {
            if (info.Current == null && info.Arguments.Length >= 1) {
                info.Current = info.Arguments[0];
            }

            //  We need to store the Format and the Current items and keep them in this context
            string format = info.Format;
            object current = info.Current;


            // ' Here is the regular expression to use for parsing the Format string:
            // Static R As New Regex( _
            //   "{  ([0-9A-Za-z_.\[\]()]*)   (?:    :  ( (?:    (?<open>{)     |     (?<nest-open>})     |     [^{}]+     )*? ) (?(open)(?!))  )?  }" _
            //   , RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)
            //   {  (      Selectors     )   (Optnl :           {  Nested                         }     or      Format                         )   }


            int lastAppendedIndex = 0;
            PlaceholderInfo placeholder = null;

            while (NextPlaceholder(format, lastAppendedIndex, format.Length, ref placeholder)) {
                //  Write the text in-between placeholders:
                info.WriteRegularText(format, lastAppendedIndex, (placeholder.placeholderStart - lastAppendedIndex));
                lastAppendedIndex = placeholder.placeholderStart + placeholder.placeholderLength;

                //  Evaluate the source by evaluating each argSelector:
                info.Current = current; //  Restore the current scope
                //bool isFirstSelector = true; // TODO: Remove this variable if it never gets used again
                int selectorIndex = -1;

                foreach (string selector in placeholder.selectors) {
                    selectorIndex++;
                    info.SetSelector(selector, selectorIndex);
                    //  Raise the ExtendCustomSource event to allow custom source evaluation:
                    OnExtendSourceEvent(new ExtendSourceEventArgs(info));

                    //  Make sure that the selector has been handled:
                    if (!info.Handled) {
                        break;
                    }
                    //isFirstSelector = false;
                }

                //  Handle errors:
                if (!info.Handled) {
                    //  If the ExtendCustomSource event wasn't handled,
                    //  then the Selector could not be evaluated.
                    if (!OnInvalidSelector(format, info, placeholder)) {
                        continue;
                    }
                }

                string argFormat = format.Substring(placeholder.formatStart, placeholder.formatLength);
                info.SetFormat(argFormat, placeholder.hasNested);

                try {
                    //  Raise the ExtendCustomFormat event to allow custom formatting:
                    OnExtendFormatEvent(new ExtendFormatEventArgs(info));
                } 
                catch (Exception ex) {
                    //  Handle errors:
                    OnInvalidFormat(format, info, placeholder, ex);
                }
            }
            //  Write the substring between the last bracket and the end of the string:
            info.WriteRegularText(format, lastAppendedIndex, (format.Length - lastAppendedIndex));
        }

        #endregion
   

        #region NextPlaceholder

        /// <summary>
        /// Returns True if the placeholder was formatted correctly; False if a placeholder couldn't be found.
        /// Outputs all relevant placeholder information.
        /// 
        /// This function takes the place of the Regular Expression.
        /// It is faster and more direct, and does not suffer from Regex endless loops.
        /// In tests, this nearly doubles the speed vs Regex.
        /// </summary>
        private bool NextPlaceholder(string format, int startIndex, int endIndex, ref PlaceholderInfo placeholder) 
        {
            placeholder = new PlaceholderInfo();
            placeholder.hasNested = false;
            placeholder.placeholderStart = -1;
            placeholder.selectorLength = -1;
            List<string> selectorSplitList = new List<string>();

            int lastSplitIndex = 0;
            int openCount = 0;
            // Dim endIndex% = format.Length

            while (startIndex < endIndex) {
                char c = format[startIndex];
                if (placeholder.placeholderStart == -1) {
                    //  Looking for "{"
                    if (c == '{') {
                        placeholder.placeholderStart = startIndex;
                        placeholder.selectorStart = startIndex + 1;
                        lastSplitIndex = placeholder.selectorStart;
                    }
                    else if (c == Plugins.Core.ParsingServices.escapeCharacter) {
                        //  The next character is escaped
                        startIndex++;
                    }
                }
                else if (placeholder.selectorLength == -1) {
                    //  Looking for ":" or "}" ...
                    //  or an alpha-numeric or a selectorSplitter
                    if (c == '}') {
                        //  Add this item to the list of Selectors (as long as it isn't empty)
                        if (lastSplitIndex < startIndex) {
                            selectorSplitList.Add(format.Substring(lastSplitIndex, (startIndex - lastSplitIndex)));
                            lastSplitIndex = (startIndex + 1);
                        }
                        placeholder.selectors = selectorSplitList.ToArray();
                        placeholder.placeholderLength = startIndex + 1 - placeholder.placeholderStart;
                        placeholder.selectorLength = startIndex - placeholder.selectorStart;
                        placeholder.formatLength = 0;
                        return true;
                    }
                    else if (c == ':') {
                        //  Add this item to the list of Selectors (as long as it isn't empty)
                        if (lastSplitIndex < startIndex) {
                            selectorSplitList.Add(format.Substring(lastSplitIndex, (startIndex - lastSplitIndex)));
                            lastSplitIndex = startIndex + 1;
                        }
                        placeholder.selectors = selectorSplitList.ToArray();
                        placeholder.selectorLength = startIndex - placeholder.selectorStart;
                        placeholder.formatStart = startIndex + 1;
                    }
                    else if (Plugins.Core.ParsingServices.selectorSplitters.IndexOf(c) >= 0) {
                        //  It is a valid splitter character
                        //  Add this item to the list of Selectors (as long as it isn't empty)
                        if (lastSplitIndex < startIndex) {
                            selectorSplitList.Add(format.Substring(lastSplitIndex, (startIndex - lastSplitIndex)));
                            lastSplitIndex = (startIndex + 1);
                        }
                    }
                    else if (char.IsLetterOrDigit(c) || Plugins.Core.ParsingServices.selectorCharacters.Contains(new string(c, 1))) {
                        //  It is a valid selector character, so let's just continue
                    }
                    else {
                        //  It is NOT a valid character!!!
                        if (placeholder.selectorStart <= startIndex) {
                            startIndex--;
                        }
                        placeholder.placeholderStart = -1; //  Restart the search
                        selectorSplitList.Clear();
                    }
                }
                else {
                    //  We are in the Format section:
                    //  Looking for a "}"
                    if (c == '}') {
                        if (openCount == 0) {
                            // we're done!
                            placeholder.placeholderLength = startIndex + (1 - placeholder.placeholderStart);
                            placeholder.formatLength = startIndex - placeholder.formatStart;
                            return true;
                        }
                        else {
                            openCount--;
                        }
                    }
                    else if (c == '{') {
                        //  It's a nested bracket
                        openCount++;
                        placeholder.hasNested = true;
                    }
                    else {
                        //  It's just part of the Format
                    }
                }
                startIndex++;
            }
            return false; // We couldn't find a full placeholder.
        }

        #endregion

    }
}