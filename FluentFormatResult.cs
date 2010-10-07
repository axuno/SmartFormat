using System;
using System.IO;
using System.Text;
using StringFormatEx.Plugins.Core;



namespace StringFormatEx
{
    public class FluentFormatResult
    {
        private readonly ExtendedStringFormatter _formatter;
        private readonly string _format;
        private readonly object[] _args;


        internal FluentFormatResult(ExtendedStringFormatter formatter, string format, object[] args)
        {
            _formatter = formatter;
            _format = format;
            _args = args;
        }


        public static implicit operator string (FluentFormatResult fluentFormatResult)
        {
            return fluentFormatResult.ToString();
        }

        public override string ToString()
        {
            StringWriter output = new StringWriter(new StringBuilder((_format.Length * 2)));
            //  Guessing a length can help performance a little.
            _formatter.FormatExInternal(new CustomFormatInfo(_formatter, output, _format, _args));
            return output.ToString();
        }

        public void Into(Stream output)
        {
            _formatter.FormatExInternal(new CustomFormatInfo(_formatter, new StreamWriter(output), _format, _args));
        }

        public void Into(TextWriter output)
        {
            _formatter.FormatExInternal(new CustomFormatInfo(_formatter, output, _format, _args));
        }

        public void Into(StringBuilder output)
        {
            Into(new StringWriter(output));
        }
    }


}