using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Demo.Sample_Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Demo;

public partial class SmartFormatDemo : Form
{
    public SmartFormatDemo()
    {
        InitializeComponent();
    }

    private RTFOutput _rtfOutput = null!;

    private PropertyGridObject _args = null!;

    private void SmartFormatDemo_Load(object sender, EventArgs e)
    {
        _args = new PropertyGridObject();
        var nestedColors = new[] {
            Color.Orange,
            Color.LightYellow,
            Color.LightGreen,
            Color.LightCyan,
            Color.LightSkyBlue,
            Color.Lavender,
        };
        _rtfOutput = new RTFOutput(nestedColors, Color.LightPink);

        _args.Person = TestFactory.GetPerson();
        _args.Date = DateTime.Now;
        _args.DateTimeOffset = DateTimeOffset.Now - new TimeSpan(1, 0,0,0);
        _args.Inventory = TestFactory.GetItems();
        _args.Xml = XElement.Parse(XmlSourceTest.TwoLevelXml);
        propertyGrid1.SelectedObject = _args;

        Smart.Default = Smart.CreateDefaultSmartFormat(new SmartSettings
        {
            Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.OutputErrorInResult},
            Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError}
        });
        Smart.Default.AddExtensions(new TimeFormatter());
        Smart.Default.AddExtensions(new XmlSource());
        Smart.Default.AddExtensions(new XElementFormatter());

        LoadExamples();
    }
    private void LoadExamples()
    {
        lstExamples.DisplayMember = "Key";
        //this.lstExamples.ValueMember = "Value";
        var examples = new Dictionary<string, string> {
            {"Basics of SmartFormat",
                @"Basics of SmartFormat
Similar to String.Format, SmartFormat uses curly braces to identify a placeholder:  The arguments on the right side of this window can be referenced in a template as follows:
{Person}, {Date}

Many .NET objects can be formatted in a specific or custom way by using a ""format string"", which is any text that comes after a colon : in a placeholder:
Long date format: {Date:D}
Short date format: {Date:d}
Custom format: {Date:""today is"" dddd, ""the"" d ""of"" MMMM}

Also works with DateTimeOffset types: 
Yesterday Local Time: {DateTimeOffset}
Yesterday Universal Time: {DateTimeOffset.UtcNow.DateTime}
Express Local Time offset to UTC with TimeFormatter: {DateTimeOffset.Offset:time(hours noless)}

For more information on Composite Formatting and standard formatting strings, please visit https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting
"},

            {"Named Placeholders", 
                @"Named Placeholders
Placeholders can use the name of any property, method, or field:
{Person.Name.ToUpper} is {Person.Age} years old and he has {Person.Friends.Count} friends.

Nested properties can use: 
- dot-notation: {Person.Address.State}
- nested notation: {Person.Address:{State}}
- any mixture of the two: {Person.Address:{State.ToString:{ToUpper}} }
"},

            {"Pluralization, Grammatical Numbers, and Gender Conjugation", 
                @"Pluralization, Grammatical Numbers, and Gender Conjugation
Many languages have specific and complex rules for pluralization and gender conjugation.  It is typically difficult to use templates and still use correct conjugation.  However, SmartFormat has a library of rules for hundreds of languages, and has an extremely simple syntax for choosing the correct word based on a value!

In English, there are only 2 plural forms: singular and plural.  You can specify a format string with both words, and the correct word will be chosen:
{Person.Random}: {Person.Random:singular|plural}

Example:
There {Person.Random:is|are} {Person.Random} {Person.Random:item|items} remaining.
"},

            {"List Formatting", 
                @"<!-- Curly braces can be escaped with a backslash -->
<Items count=""{Inventory.Count}"">
{Inventory:
    <Item name=""{Name}"" price=""{Price:c}"" index=""{Index}"" components=""{Components.Count}"">
    {Components:
        <Component name=""{Name}"" count=""{Count}"" />
    |}
    </Item>
|}
</Items>
"},
            {"Xml Source", 
                @"It is possible to format Xml as input argument
Example:
  There are {Xml.Person.Count} people: {Xml.Person: {FirstName}|,|, and}
  #1:  {Xml.Person[0]: {FirstName}'s phone number is {Phone}}
  #2:  {Xml.Person[1]: {FirstName}'s phone number is {Phone}}
"},
        };

        var listObjects = examples.Cast<object>().ToArray();
        lstExamples.Items.AddRange(listObjects);
        lstExamples.SelectedIndex = 0;
    }

    public class PropertyGridObject
    {
        // We want the items in the Property Grid to be expandable,
        // so we need each property to have a TypeConverterAttribute:
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public object Person { get; set; } = new Person();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public object Date { get; set; } = DateTime.Now;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public object DateTimeOffset { get; set; } = new DateTimeOffset(DateTime.Now);

        [TypeConverter(typeof(ArrayConverter))]
        public object Inventory { get; set; } = new object();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public object Xml { get; set; } = new object();
    }

    private void TxtInput_TextChanged(object sender, EventArgs e)
    {
        groupBox1.ResetForeColor();
        groupBox1.Text = "Format";
        var format = txtInput.Text;

        _rtfOutput.Clear();
        // Save selection:
        var s = txtInput.SelectionStart;
        var l = txtInput.SelectionLength;
        try
        {
            Smart.Default.FormatInto(_rtfOutput, format, _args);

            txtInput.SelectAll();
            txtInput.SelectionBackColor = txtInput.BackColor;
        }
        catch (ParsingErrors ex)
        {
            var errorBg = Color.LightPink;
            var errorFg = Color.DarkRed;
            groupBox1.ForeColor = errorFg;

            groupBox1.Text = string.Format("Format has {0} issue{1}: {2}",
                ex.Issues.Count,
                (ex.Issues.Count == 1) ? "" : "s",
                ex.Issues.Select(i=>i.Issue).JoinStrings(" ", " ", " (and {0} more)", 1)
            );
            // Highlight errors:
            foreach (var issue in ex.Issues)
            {
                txtInput.Select(issue.Index, issue.Length);
                txtInput.SelectionBackColor = errorBg;
            }
        }
        txtInput.SelectionStart = s;
        txtInput.SelectionLength = l;

        txtOutput.Rtf = _rtfOutput.ToString();
    }

    private void LstExamples_SelectedIndexChanged(object sender, EventArgs e)
    {
        var example = (KeyValuePair<string, string>) lstExamples.SelectedItem;
        if (example.Value == null) return;
        txtInput.Text = example.Value;
    }
}
