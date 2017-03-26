


 //using CurrentPatient.Properties;


namespace RTF
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;


    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:34
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      RTFBuilder.cs - Code
    //        Purpose:                Rich Text Generator
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Rich Text Generator
    /// </summary>
    public partial class RTFBuilder : RTFBuilderbase
    {
        #region Fields

        private static readonly char[] slashable = new[] {'{', '}', '\\'};

        private readonly StringBuilder _sb;

        #endregion

        #region Constructor

        public RTFBuilder()
            : base(RTFFont.Arial , 20F)
        {
            this._sb = new StringBuilder();
        }

        public RTFBuilder(RTFFont defaultFont) : base(defaultFont, 20F)
        {
            this._sb = new StringBuilder();
        }

        public RTFBuilder(float defaultFontSize) : base(RTFFont.Arial, defaultFontSize)
        {
            this._sb = new StringBuilder();
        }

        public RTFBuilder(RTFFont defaultFont, float defaultFontSize) : base(defaultFont, defaultFontSize)
        {
            this._sb = new StringBuilder();
        }

        #endregion

        #region Override Methods

        protected override void AppendInternal(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                using (new RTFFormatWrap(this))
                {
                    value = this.CheckChar(value);
                    if (value.Contains("\n"))
                    {
                        string[] lines = value.Split(new[] {"\r\n","\n"}, StringSplitOptions.None);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (i > 0) this._sb.Append("\\line ");
                            this._sb.Append(lines[i]);
                        }
                    }
                    else
                    {
                        this._sb.Append(value);
                    }
                }
            }
        }

        protected override void AppendLevelInternal(int level)
        {
            this._sb.AppendFormat("\\level{0} ", level);
        }

        protected override void AppendLineInternal(string value)
        {
            using (new RTFParaWrap(this))
            {
                Append(value);
                this._sb.AppendLine("\\line");
            }
        }

        protected override void AppendLineInternal()
        {
            this._sb.AppendLine("\\line");
        }

        protected override void AppendPageInternal()
        {
            using (new RTFParaWrap(this))
            {
                this._sb.AppendLine("\\page");
            }
        }

        protected override void AppendParaInternal()
        {
            using (new RTFParaWrap(this))
            {
                this._sb.AppendLine("\\par ");
            }
        }

        protected override void AppendRTFInternal(string rtf)
        {
            if (!string.IsNullOrEmpty(rtf))
            {
                this._sb.Append(rtf);
            }
        }

        protected override IEnumerable <RTFBuilderbase> EnumerateCellsInternal(RTFRowDefinition rowDefinition, RTFCellDefinition[] cellDefinitions)
        {
            using (IRTFRow ie = this.CreateRow(rowDefinition, cellDefinitions))
            {
                IEnumerator <IBuilderContent> ie2 = ie.GetEnumerator();
                while (ie2.MoveNext())
                {
                    using (IBuilderContent item = ie2.Current)
                    {
                        yield return item.Content;
                    }
                }
            }
        }

        public override IDisposable FormatLock()
        {
            return new RTFBuilderUnWrapped(this);
        }

        protected override void InsertImageInternal(Image image)
        {
            try
            {
                RTFImage rti = new RTFImage(this);
                rti.InsertImage(image);
            }
            catch
            {
                this._sb.AppendLine("[Insert image error]");
            }
        }

        protected override int LengthInternal()
        {
            throw new NotImplementedException();
        }

        protected override void ResetInternal()
        {
            this._sb.AppendLine("\\pard");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081");
            sb.Append("{\\fonttbl");


            for (int i = 0; i < _rawFonts.Count; i++)
            {

                try
                {
                    sb.Append(string.Format(_rawFonts[i], i));
                }
                catch (Exception ex )
                {

                    Console.WriteLine(ex.Message );
                }

            }

            sb.AppendLine("}");

            sb.Append("{\\colortbl ;");

            foreach (Color item in _colortbl)
            {
                sb.AppendFormat("\\red{0}\\green{1}\\blue{2};", item.R, item.G, item.B);
            }

            sb.AppendLine("}");


            sb.Append("\\viewkind4\\uc1\\pard\\plain\\f0");

            sb.AppendFormat("\\fs{0} ", DefaultFontSize);
            sb.AppendLine();

            sb.Append(this._sb.ToString());
            sb.Append("}");


            return sb.ToString();
        }

        #endregion

        #region Public Methods

        public IRTFRow CreateRow(RTFRowDefinition rowDefinition, RTFCellDefinition[] cellDefinitions)
        {
            return new RTFRow(this, rowDefinition, cellDefinitions);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks the char.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string CheckChar(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.IndexOfAny(slashable) >= 0)
                {
                    value = value.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
                }
                bool replaceuni = false;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] > 255)
                    {
                        replaceuni = true;
                        break;
                    }
                }
                if (replaceuni)
                {
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] <= 255)
                        {
                            sb.Append(value[i]);
                        }
                        else
                        {
                            sb.Append("\\u");
                            sb.Append((int) value[i]);
                            sb.Append("?");
                        }
                    }
                    value = sb.ToString();
                }
            }


            return value;
        }

        #endregion
    }
}

