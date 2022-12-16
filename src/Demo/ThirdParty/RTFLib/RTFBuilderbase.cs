
namespace RTF
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Drawing;
    using System.Text;
    using System.Text.RegularExpressions;

    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:23
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      RTFBuilderbase.cs - Code
    //        Purpose:                The Main base class of this project
    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Base Class for RtfBuilder and GDF Builder
    /// Handles format (font , fontstyle, colour) delegating string appending to derived classes. 
    /// </summary>
    public abstract class RTFBuilderbase : IDisposable
    {
 
        #region Fields
        private readonly Color _defaultbackcolor;
        private readonly float _defaultFontSize;
        private readonly Color _defaultforecolor;
        protected Color _backcolor;
        protected List <Color> _colortbl;
        protected int _firstLineIndent;
        protected int _font;
        protected  StringCollection _rawFonts;
        protected float _fontSize;
        protected FontStyle _fontStyle;
        /// <summary>
        /// 
        /// </summary>
        protected List <string> _fontTable;
        protected Color _forecolor;
        protected int _lineIndent;
        protected StringFormat _sf;
        protected bool _unwrapped;
        #endregion
        #region Constructor
        protected RTFBuilderbase(RTFFont  defaultFont, float defaultFontSize)
        {

            _rawFonts = new StringCollection();
            _font = IndexOfFont(defaultFont);
            _defaultFontSize = defaultFontSize;
            _fontSize = _defaultFontSize;
            _defaultforecolor = SystemColors.WindowText;
            _defaultbackcolor = SystemColors.Window;
            _colortbl = new List <Color>();
            _colortbl.Add(_defaultforecolor);
            _colortbl.Add(_defaultbackcolor);
            _fontStyle = System.Drawing.FontStyle.Regular;
            _backcolor = _defaultbackcolor;
            _forecolor = _defaultforecolor;
            _sf = (StringFormat) StringFormat.GenericDefault.Clone();
            _sf.FormatFlags = StringFormatFlags.NoWrap;
            _sf.Trimming = StringTrimming.Word;
        }
        #endregion
        #region Public Properties
        public int Length
        {
            get { return LengthInternal(); }
        }
        public float DefaultFontSize
        {
            [DebuggerStepThrough]
            get { return _defaultFontSize; }
        }
        public Color Defaultforecolor
        {
            [DebuggerStepThrough]
            get { return _defaultforecolor; }
        }
        public List <RTFFont> Fonts
        {
            [DebuggerStepThrough]
            get { return null; }
        }
        public Color DefaultBackColor
        {
            [DebuggerStepThrough]
            get { return _defaultbackcolor; }
        }
        #endregion
        #region Virtual Methods

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
        #region Override Methods

        ~RTFBuilderbase()
        {
            Dispose(false);
        }

        #endregion
        #region Abstract Methods

        protected abstract void AppendInternal(string value);
        protected abstract void AppendLevelInternal(int level);
        protected abstract void AppendLineInternal(string value);
        protected abstract void AppendLineInternal();
        protected abstract void AppendPageInternal();
        protected abstract void AppendParaInternal();
        protected abstract void AppendRTFInternal(string rtf);
        protected abstract IEnumerable <RTFBuilderbase> EnumerateCellsInternal(RTFRowDefinition rowDefinition, RTFCellDefinition[] cellDefinitions);
        public abstract IDisposable FormatLock();
        protected abstract void InsertImageInternal(Image image);
        protected abstract int LengthInternal();
        protected abstract void ResetInternal();

        #endregion
        #region Public Methods
        public RTFBuilderbase Alignment(StringAlignment alignment)
        {
            _sf.Alignment = alignment;
            return this;
        }
        public RTFBuilderbase Append(string value)
        {
            AppendInternal(value);
            return this;
        }
        public RTFBuilderbase Append(char[] value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(value);
            return Append(sb.ToString());
        }
        public RTFBuilderbase Append(short value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(double value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(float value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(int value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(string value, int startIndex, int count)
        {
            return Append(value.Substring(startIndex, count));
        }
        public RTFBuilderbase Append(char value, int repeatCount)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(value, repeatCount);
            return Append(sb.ToString());
        }
        public RTFBuilderbase Append(long value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(object value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(bool value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(byte value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(decimal value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase Append(char value)
        {
            return Append(value.ToString());
        }
        public RTFBuilderbase AppendFormat(string format, object arg0)
        {
            string formated = string.Format(format, arg0);
            return Append(formated);
        }
        public RTFBuilderbase AppendFormat(string format, params object[] args)
        {
            string formated = string.Format(format, args);
            return Append(formated);
        }
        public RTFBuilderbase AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            string formated = string.Format(provider, format, args);
            return Append(formated);
        }
        public RTFBuilderbase AppendFormat(string format, object arg0, object arg1)
        {
            string formated = string.Format(format, arg0, arg1);
            return Append(formated);
        }
        public RTFBuilderbase AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            string formated = string.Format(format, arg0, arg1, arg2);
            return Append(formated);
        }
        public RTFBuilderbase AppendLevel(int level)
        {
            AppendLevelInternal(level);
            return this;
        }
        public RTFBuilderbase AppendLine(string value)
        {
            AppendLineInternal(value);
            return this;
        }
        public RTFBuilderbase AppendLine()
        {
            AppendLineInternal();
            return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase AppendLineFormat(string format, object arg0)
        {
            string formated = string.Format(format, arg0);
            return AppendLine(formated);
        }
        [DebuggerStepThrough]
        public RTFBuilderbase AppendLineFormat(string format, params object[] args)
        {
            string formated = string.Format(format, args);
            return AppendLine(formated);
        }
        [DebuggerStepThrough]
        public RTFBuilderbase AppendLineFormat(string format, object arg0, object arg1, object arg2)
        {
            string formated = string.Format(format, arg0, arg1, arg2);
            return AppendLine(formated);
        }
        public RTFBuilderbase AppendPage()
        {
            AppendPageInternal();
            return this;
        }
        public RTFBuilderbase AppendPara()
        {
            AppendParaInternal();
            return this;
        }
        public RTFBuilderbase AppendRTF(string rtf)
        {
            AppendRTFInternal(rtf);
            return this;
        }
        /// <summary>
        /// Appends the RTF document.
        /// </summary>
        /// <param name="rtf">The RTF.</param>
        /// <returns></returns>
        public RTFBuilderbase AppendRTFDocument(string rtf)
        {
            if (!string.IsNullOrEmpty(rtf))
            {
                if (rtf.IndexOf("viewkind4") > 0)
                {
                    try
                    {
                        string rtfc = GetColoursFromRtf(rtf);
                        string rtff = GetFontsFromRtf(rtfc);
                        string texttoadd = GetConcentratedText(rtff);
                        AppendRTF(texttoadd);
                    }
                    catch (ArgumentException ex)
                    {
                        AppendLine("RTF Document ERROR:");
                        AppendLine(ex.Message);
                    }
                }
            }
            return this;
        }
        private static string GetConcentratedText(string rtf)
        {
            string find = @"\viewkind4";
            int start = rtf.IndexOf(find);
            start = rtf.IndexOf("\\pard", start);
            int end = rtf.LastIndexOf("}");
            int end2 = rtf.LastIndexOf("\\par");
            if (end2 > 0 && ((end - end2) < 8))
            {
                end = end2;
            }
            return rtf.Substring(start + 5, end - start - 5);
        }

        private string GetColoursFromRtf(string rtf)
        {
            Regex RegexObj = new Regex("\\{\\\\colortbl ;(?<colour>\\\\red(?<red>\\d{0,3})\\\\green(?<green>\\d{0,3})\\\\blue(?<blue>\\d{0,3});)*\\}");
            Match MatchResults = RegexObj.Match(rtf);
            Dictionary <int, int> replaces = new Dictionary <int, int>();
            while (MatchResults.Success)
            {
                Group GroupObj = MatchResults.Groups["red"];
                if (GroupObj.Success)
                {
                    for (int i = 0; i < GroupObj.Captures.Count; i++)
                    {
                        int redval, greenval, blueval;
                        if (int.TryParse(MatchResults.Groups["red"].Captures[i].Value, out redval) &&
                            int.TryParse(MatchResults.Groups["green"].Captures[i].Value, out greenval) &&
                            int.TryParse(MatchResults.Groups["blue"].Captures[i].Value, out blueval))
                        {
                            Color c = Color.FromArgb(redval, greenval, blueval);
                            int count = _colortbl.Count;
                            int index = IndexOf(c);
                            if ((i + 1) == index)
                            {
                                continue;
                            }
                            replaces.Add(i + 1, index);
                        }
                    }
                }
                MatchResults = MatchResults.NextMatch();
            }
            if (replaces.Count > 0)
            {

                    //delegate string MatchEvaluator(System.Text.RegularExpressions.Match match)
                    rtf = Regex.Replace(rtf, "(?:\\\\cf)([0-9]{1,2})", delegate(Match match)
                                                                           {
                                                                               int val;
                                                                               if (int.TryParse(match.Groups[1].Value, out val) && val > 0)
                                                                               {
                                                                                       if (replaces.ContainsKey(val))
                                                                                       {
                                                                                           return string.Format(@"\cf{0}", replaces[val]);
                                                                                       }
                                                                               }
                                                                               return match.Value;
                                                                           });

                    //delegate string MatchEvaluator(System.Text.RegularExpressions.Match match)
                    rtf = Regex.Replace(rtf, "(?:\\\\highlight)([0-9]{1,2})", delegate(Match match)
                                                                           {
                                                                               int val;
                                                                               if (int.TryParse(match.Groups[1].Value, out val) && val > 0)
                                                                               {
                                                                                       if (replaces.ContainsKey(val))
                                                                                       {
                                                                                           return string.Format(@"\highlight{0}", replaces[val]);
                                                                                       }
                                                                               }
                                                                               return match.Value;
                                                                           });

            }
            return rtf;
        }
     
        private string GetFontsFromRtf(string rtf)
        {
            // Regex that almost works for MS Word
            // \{\\fonttbl(?<raw>\{\\f(?<findex>\d{1,3})(\\fbidi )?\\(?<fstyle>fswiss|fnil|froman|fmodern|fscript|fdecor)\\fcharset(?<fcharset>\d{1,3}) ?(?<fprq>\\fprq\d{1,3} ?)(\{\\\*\\panose [\dabcdef]{20}\}\d?)?(?<FONT>[@\w -]+)(\\'\w\w)?(\{\\\*\\falt \\'\w\w\\'\w\w\\'\w\w\\'\w\w\})?;\}(\r\n)?)+
            Regex RegexObj = new Regex(@"\{\\fonttbl(?<raw>\{\\f(?<findex>\d{1,3}) ?\\(?<fstyle>fswiss|fnil|froman|fmodern|fscript|fdecor|ftech|fbidi) ?(?<fprq>\\fprq\d{1,2} ?)?(?<fcharset>\\fcharset\d{1,3})? (?<FONT>[\w -]+);\})+\}\r?\n", RegexOptions.Multiline);
            Match MatchResults = RegexObj.Match(rtf);
            Dictionary<int, int> replaces = new Dictionary<int, int>();
            Group GroupObj = MatchResults.Groups["raw"];
            for (int i = 0; i <GroupObj.Captures.Count;i++)
            {
                string raw = GroupObj.Captures[i].Value;
                //string font = MatchResults.Groups["FONT"].Captures[newdocindex].Value;
                Capture cap =MatchResults.Groups["findex"].Captures[i];
     
                //have to replace findex with {0}
                raw =string.Concat( "{", Regex.Replace(raw, "\\{\\\\f\\d{1,3}", "{\\f{0}"),"}");
 
                string curdocstringindex = MatchResults.Groups["findex"].Captures[i].Value;
                

                int sourceindex;
                if (int.TryParse(curdocstringindex, out sourceindex))
                {

                    int destinationindex = IndexOfRawFont(raw);
                    if (destinationindex != sourceindex)
                    {
                        replaces.Add(sourceindex, destinationindex);
                    }
                }
            }
            if (replaces.Count > 0)
            {


                        rtf = Regex.Replace(rtf, "(?<!\\{)(\\\\f)(\\d{1,3})( ?|\\\\)", match => 
                        {
                            int sourceindex2;
                            if (int.TryParse(match.Groups[2].Value, out sourceindex2))
                            {
                                if (replaces.ContainsKey(sourceindex2))
                                {
                                    string rep=  string.Format("\\f{0}{1}", replaces[sourceindex2], match.Groups[3].Value);
                                   return rep;
                                }
                            } 
                            return match.Value;
                        });

            }
            return rtf;
        }
        /// <summary>
        /// Indexes the of font.
        /// </summary>
        /// <param name="font">The font.</param>
        /// <returns></returns>
        public int IndexOfFont(RTFFont  font)
        {
           return IndexOfRawFont(RTFBuilder.RawFonts.GetKnownFontstring(font));
        }
        private int IndexOfRawFont(string font)
        {
            if (!string.IsNullOrEmpty(font))
            {
                int index = _rawFonts.IndexOf(font);
                if (index < 0)
                {
                    return _rawFonts.Add(font);
                }
                return index;
            }
            return 0;
        }

        [DebuggerStepThrough]
        public RTFBuilderbase BackColor(KnownColor color)
        {
            return BackColor(Color.FromKnownColor(color));
        }
        [DebuggerStepThrough]
        public RTFBuilderbase BackColor(Color color)
        {
            _backcolor = color;
            return this;
        }
        public void Clear()
        {
        }
        public String ComputeReplacement(Match m)
        {
            // You can vary the replacement text for each match on-the-fly
            return "";
        }
        public IEnumerable <RTFBuilderbase> EnumerateCells(RTFRowDefinition rowDefinition, RTFCellDefinition[] cellDefinitions)
        {
            return EnumerateCellsInternal(rowDefinition, cellDefinitions);
        }
        [DebuggerStepThrough]
        public RTFBuilderbase Font(RTFFont font)
        {
           _font = IndexOfFont(font);
           return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase Font(int index)
        {
            if (index >= 0 && index < _rawFonts.Count)
            {
                _font = index;
            }
            return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase FontSize(float fontSize)
        {
            _fontSize = fontSize;
            return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase ForeColor(KnownColor color)
        {
            _forecolor = Color.FromKnownColor(color);
            return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase ForeColor(Color color)
        {
            _forecolor = color;
            return this;
        }
        public RTFBuilderbase ForeColor(int index)
        {
            index--;
            if (index >= 0 && index < _colortbl.Count)
            {
                _forecolor = _colortbl[index];
            }
            else if (index < 0)
            {
                _forecolor = _colortbl[0];
            }
            return this;
        }
        /// <summary>
        /// Changes the Font Style.
        /// </summary>
        /// <param name="fontStyle">The font style.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public RTFBuilderbase FontStyle(FontStyle fontStyle)
        {
            _fontStyle = fontStyle;
            return this;
        }
        [DebuggerStepThrough]

        public RTFBuilderbase FontStyleNOT(FontStyle fontStyle)
        {
            if ((_fontStyle & fontStyle) == fontStyle)
            {
                _fontStyle &= ~fontStyle;
            }
            return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase FontStyleOR(FontStyle fontStyle)
        {
            _fontStyle |= fontStyle;
            return this;
        }
        /// <summary>
        /// Gets the index of the Color.
        /// Important for merging ColorTables
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public int IndexOf(Color color)
        {
            if (!_colortbl.Contains(color))
            {
                _colortbl.Add(color);
            }
            int index = _colortbl.IndexOf(color) + 1;
            return index;
        }
        /// <summary>
        /// Inserts the image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public RTFBuilderbase InsertImage(Image image)
        {
            InsertImageInternal(image);
            return this;
        }
        public RTFBuilderbase LineIndent(int indent)
        {
            _lineIndent = indent;
            return this;
        }
        public RTFBuilderbase LineIndentFirst(int indent)
        {
            _firstLineIndent = indent;
            return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase PrependLineFormatIf(string format, ref bool appended, object arg0)
        {
            string formated = string.Format(format, arg0);
            return PrependLineIf(formated, ref appended);
        }
        [DebuggerStepThrough]
        public RTFBuilderbase PrependLineFormatIf(string format, ref bool appended, params object[] args)
        {
            string formated = string.Format(format, args);
            return PrependLineIf(formated, ref appended);
        }
        [DebuggerStepThrough]
        public RTFBuilderbase PrependLineFormatIf(string format, ref bool appended, object arg0, object arg1, object arg2)
        {
            string formated = string.Format(format, arg0, arg1, arg2);
            return PrependLineIf(formated, ref appended);
        }
        [DebuggerStepThrough]
        public RTFBuilderbase PrependLineIf(ref bool appended)
        {
            if (appended)
            {
                AppendLine();
            }
            appended = true;
            return this;
        }
        [DebuggerStepThrough]
        public RTFBuilderbase PrependLineIf(string value, ref bool appended)
        {
            if (appended)
            {
                AppendLine();
                Append(value);
            }
            else
            {
                Append(value);
            }
            appended = true;
            return this;
        }
        public RTFBuilderbase Reset()
        {
            ResetInternal();
            return this;
        }
        #endregion
        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
