using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SmartFormat.Core;
using SmartFormat.Core.Parsing;
using SmartFormat.Demo.Sample_Extensions;
using SmartFormat.Tests;
using SmartFormat.Tests.Common;

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


            arg0 = TestFactory.GetPerson();
            arg1 = DateTime.Now;

            propertyGrid1.SelectedObject = new PropertyGridObject() {
                                                                        arg0 = arg0,
                                                                        arg1 = arg1,
                                                                    };


            Smart.Default.ErrorAction = ErrorAction.OutputErrorInResult;
            Smart.Default.Parser.ErrorAction = ErrorAction.ThrowError;
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

            
            groupBox1.ResetForeColor();
            groupBox1.Text = "Format";
            var format = richTextBox1.Text;

            rtfOutput.Clear();
            // Save selection:
            var s = richTextBox1.SelectionStart;
            var l = richTextBox1.SelectionLength;
            try
            {
                Smart.Default.FormatInto(rtfOutput, format, arg0, arg1);
                
                richTextBox1.SelectAll();
                richTextBox1.SelectionBackColor = richTextBox1.BackColor;
            }
            catch (ParsingErrors ex)
            {
                var errorBG = Color.LightPink;
                var errorFG = Color.DarkRed;
                groupBox1.ForeColor = errorFG;

                groupBox1.Text = string.Format("Format has {0} issue{1}: {2}", 
                                               ex.Issues.Count,
                                               (ex.Issues.Count == 1) ? "" : "s",
                                               ex.Issues.Select(i=>i.Issue).JoinStrings(" ", " ", " (and {0} more)", 1)
                                               );
                // Highlight errors:
                foreach (var issue in ex.Issues)
                {
                    richTextBox1.Select(issue.Index, issue.Length);
                    richTextBox1.SelectionBackColor = errorBG;
                }
            }
            richTextBox1.SelectionStart = s;
            richTextBox1.SelectionLength = l;

            richTextBox2.Rtf = rtfOutput.ToString();
        }


    }
}
