using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SmartFormat.Core;
using SmartFormat.Demo.Sample_Plugins;
using SmartFormat.Tests;

namespace SmartFormat.Demo
{
    public partial class SmartFormatTest : Form
    {
        public SmartFormatTest()
        {
            InitializeComponent();
        }

        private RTFOutput rtfOutput;

        private object arg0;
        private object arg1;

        private void SmartFormatTest_Load(object sender, EventArgs e)
        {
            var nestedColors = new Color[] {
                Color.Orange,
                Color.LightYellow,
                Color.LightGreen,
                Color.LightCyan,
                Color.LightSkyBlue,
                Color.Lavender,
            };
            rtfOutput = new RTFOutput(nestedColors, Color.LightPink);


            arg0 = BaseTest.GetPerson();
            arg1 = DateTime.Now;

            propertyGrid1.SelectedObject = new PropertyGridObject() {
                                                                        arg0 = arg0,
                                                                        arg1 = arg1,
                                                                    };


            Smart.Default.ErrorAction = ErrorAction.OutputErrorInResult;
            Smart.Default.Parser.ErrorAction = ErrorAction.OutputErrorInResult;
        }
        public class PropertyGridObject
        {
            // We want the items in the Property Grid to be expandable,
            // so we need each property to have a TypeConverterAttribute:
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public object arg0 { get; set; }
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public object arg1 { get; set; }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            var format = richTextBox1.Text;

            rtfOutput.Clear();
            Smart.Default.FormatInto(rtfOutput, format, arg0, arg1);

            richTextBox2.Rtf = rtfOutput.ToString();
        }


    }
}
