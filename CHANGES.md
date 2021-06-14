Latest Changes
====

v3.0.0 (Draft)
===

### Currently merged to the `version/v3.0` branch:

#### Introduced Nullable Notation

The C# like `nullable` notation allows to display `Nullable<T>` types, depending on whether a variable contains the underlying type's value or `null`. The SmartFormat notation is `"{SomeNullable?.Property}"`. If `SomeNullable` is null, the expression is evaluated as `string.empty`.

In this context the `NullFormatter` was introduced. It outputs a custom string literal, if the variable is `null`, else `string.empty`.

Putting both together:

```Csharp
var smart = new SmartFormatter();
smart.AddExtensions(new IFormatter[] { new NullFormatter() });
```
The variable is `null`:
```Csharp
// nullResult: "This value is null"
var nullResult = smart.Format("{TheValue:isnull:This value is null}", new {TheValue = null});
// valueResult: string.Empty
var valueResult = smart.Format("{TheValue?}", new {TheValue = null});
```
The variable is a `string` value:
```Csharp
// nullResult: string.Empty
nullResult = smart.Format("{TheValue:isnull:This value is null}", new {TheValue = "My string value"});
// valueResult: "My string value"
valueResult = smart.Format("{TheValue?}", new {TheValue = "My string value"});
```
In the most simple scenario, both evaluations can also be combined with the `ChooseFormatter`:
```Csharp
// result: "This value is null" or the string value
var result = smart.Format("{TheValue?:choose(null):This value is null|{}}}", new {TheValue = null});
```
In more complex scenarios, you will, however, use the nullable notation together with other formatter extensions, like a `ListFormatter`.

**Note:** Trying to evaluate `null` without the nullable operator will result in a formatting exception. This is the same behavior as in SmartFormat 2.x.

Changes connected to nullable notation:
* Source extension should have `Source` as the abstract base class, instead of `ISource`.
* Source extensions should check for `null` values together with the nullable operator and return `true` in the `bool TryEvaluateSelector(ISelectorInfo selectorInfo)` method.
* The `Parser` accepts `?` as another standard operator.
* The nullable operator can also be used evaluating a list index, e.g. `"{TheList?[1]}"`.

#### Refactored implementation for string alignment ([#174](https://github.com/axuno/SmartFormat/pull/174))
Example: `Smart.Format("{placeholder,15}")` where ",15" is the string alignment

* Aligment is now implemented into class `FormattingInfo`, so it is **always available**. 
* Introduced `FormatterSettings.AlignmentFillCharacter`, to the the fill character can be customized. Default is space (0x20), like with `string.Format`.
* Former implementation in `DefaultFormatter` is removed.
* Modified `ListFormatter` so that items can be aligned, but the spacers stay untouched
* `IFormattingInfo.Alignment` now returns the alignment of the current `Placeholder`, or - if this is null - the `Alignment` of any parent `IFormattingInfo` that is not zero.
* Renamed `IFormattingInfo.Write(Format format, object value)` to `FormatAsChild(Format format, object value)` to make clear that nothing is written to `IOutput` (this happens in a next step).
* Added dedicated AlignmentTests

#### Separated mode for SmartFormat features from `string.Format`compatibility ([#173](https://github.com/axuno/SmartFormat/pull/173), [#175](https://github.com/axuno/SmartFormat/pull/175))
**1. `string.Format` compatibility:**
   * SmartFormat acts as a drop-in replacement, and on top allows for named placeholders besides indexed placeholders. Example (note the colon is not escaped):
   * ```csharp
            var now = DateTime.Now;
            var smartFmt = "It is now {Date:yyyy/MM/dd HH:mm:ss}";
            var stringFmt = $"It is now {now.Date:yyyy/MM/dd HH:mm:ss}";
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.StringFormatCompatibility = true;
            Assert.That(formatter.Format(smartFmt, now), Is.EqualTo(stringFmt));
      ```
   
   * The `Parser` will not include the formatter name or formatting options. Like with `string.Format`, everything after the `Selector` separator (colon) is considered as format specifier.
   * Curly braces are escaped the `string.Format` way with `{{` and `}}`
   * `DefaultFormatter` is the only formatter which will be invoked. `null` will be output as `string.Empty`.
   * Even in compatibility mode, *SmartFormat* will
     * Process named `Placeholder`s (beside indexed `Placeholder`s)
     * have `ISource`s available for `Placeholder`s.
     * be able to process escaped string literals like \n, \U1234 etc.

**2. SmartFormat added feature:**
  * As long as special characters (`(){}:\`) are escaped, any character is allowed anywhere. Now this applies also for the colon. Example (note the escaped colon):
   * ```Csharp
            var now = DateTime.Now;
            var smartFmt = @"It is now {Date:yyyy/MM/dd HH\:mm\:ss}";
            var stringFmt = $"It is now {now.Date:yyyy/MM/dd HH:mm:ss}";
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.StringFormatCompatibility = false;
            Assert.That(formatter.Format(smartFmt, now), Is.EqualTo(stringFmt));
      ```
  * Tests are modified occordingly

**3. All modes**

* It is possible to use a `System.IFormatProvider` as argument to `Smart.Format` - same as with `string.Format`. This custom format provider can in turn call a `System.ICustomFormatter` for special formatting needs.
* This feature is implemented in the `DefaultFormatter`.

4. Moved `ParserSettings.StringFormatCompatibility` to `Settings.StringFormatCompatibility` because this does no more apply to the parser only.

5. Parser does not process `string[] formatterExtensionNames` any more
  * CTOR does not take `formatterExtensionNames` as argument
  * `Parser.ParseFormat` does not check for a valid formatter name (it's implemented in the formatter anyway)

---
---

* Constrain custom selectors and operators ([#172](https://github.com/axuno/SmartFormat/pull/172))
  * Custom selector chars can be added, if not disallowed or in use as an operator char
  * Custom operator chars can be added, if not disallowed or in use as a selector char
  * Alphanumeric selector chars are the only option now and cannot be degraded to pure numeric chars
  * `PlaceholderBeginChar`, `PlaceholderEndChar`, `FormatterOptionsBeginChar` and `FormatterOptionsEndChar` now only have getters
* Reduced substring usage with literal text leads to a significant reduction of GC and memory allocation ([#169](https://github.com/axuno/SmartFormat/pull/169)) 
* Add unicode escape characters ([#166](https://github.com/axuno/SmartFormat/pull/166)). Thanks to [@karljj1](https://github.com/karljj1)
* `FormatItem` abstract class ([#167](https://github.com/axuno/SmartFormat/pull/167))
  * Obsoleted public field `baseString`, changed to `BaseString` property
  * Obsoleted public field `startIndex`, changed to `StartIndex` property
  * Obsoleted public field `endIndex`, changed to `IndexIndex` property
  * Added `Length`property (returing `EndIndex` - `StartIndex`)
* Parser can parse any character as formatter options. This means e.g. no limitations for `RegEx` expressions used in `IsMatchFormatter`. ([#165](https://github.com/axuno/SmartFormat/pull/165)) 
  * Obsoleted public property `Format.parent`, changed to `Format.Parent`
  * Obsoleted public property `Placeholder.parent`, changed to `Placeholder.Parent`
  * `Placeholder` now has `FormatterOptionsRaw` with original (unescaped) string and `FormatterOptions` with the escaped string.
  * `EscapedLiteral`: 15% faster, 50% less GC
* `Parser` ([#164](https://github.com/axuno/SmartFormat/pull/164)):
  * Moved settings from class `Parser` to class `ParserSettings` and corresponding members in `Parser` are marked as obsolete.
  * `Parser.UseAlternativeBraces(..)` is obsolete and not supported any more
  * Default character for escaping curly braces is now `\`, i.e. `\{` and `\}` instead of `{{` and `}}`. The latter are used if `SmartSettings.StringFormatCompatibility` is `true`.
  * Alphanumeric selector characters are enabled by default.
  * Operator characters `.,[]` are set by default.
* `ErrorAction`s ([#164](https://github.com/axuno/SmartFormat/pull/164)):
   * Enum `SmartSettings.ErrorAction` is obsolete and replaced with `SmartSettings.ParseErrorAction` and `SmartSettings.FormatErrorAction`
   * Property `SmartSettings.ParseErrorAction` is obsolete and changed to `ParserSettings.ErrorAction`
   * Property `SmartSettings.FormatErrorAction` is obsolete and changed to `FormatterSettings.ErrorAction`
* Added `SmartFormat.Performance` project with benchmarks for different data sources ([#162](https://github.com/axuno/SmartFormat/pull/162))
* Fully cover `Parser` and `ReflectionSource` with unit tests ([#160](https://github.com/axuno/SmartFormat/pull/160))
* Removed redundant unit tests with no impact on coverage ([#159](https://github.com/axuno/SmartFormat/pull/159))
* Removed obsolete `SmartObjects` (which have been replaced by `ValueTuple`) ([`092b7b1`](https://github.com/axuno/SmartFormat/commit/092b7b1b5873301bdfeb2b62f221f936efc81430))
* Added type cache to `ReflectionSource`, `BenchmarkDotNet` is now about 4x better (thanks to [@karljj1](https://github.com/karljj1)) ([#155](https://github.com/axuno/SmartFormat/pull/155))
* Added support for `IList<object>` parameters to the `SmartFormatter` (thanks to [@karljj1](https://github.com/karljj1)) ([#154](https://github.com/axuno/SmartFormat/pull/154))

v2.7.0
===
* **Fixed** broken backward compatibilty introduced in v2.6.2 (issues referenced in [#148](https://github.com/axuno/SmartFormat/issues/148), [#147](https://github.com/axuno/SmartFormat/issues/147), [#143](https://github.com/axuno/SmartFormat/issues/143)).
* **Fixed**: Take an erroneous format string like `"this is {uncomplete"` (missing closing brace). Before v2.7.0 the parser handled `{uncomplete` as a `TextLiteral`, not as an erroneous `Placeholder`.
* **Fixed**: Since v1.6.1 there was an undiscovered issue: If the `Parser` encountered a `ParsingError.TooManyClosingBraces`, this closing brace was simply "swallowed-up". This way, the result with `Parser.ErrorAction.MaintainTokens` differs from the original format string. From v2.7.0, the redundant closing brace is handled as a `TextLiteral`. 
* **Improved**: For `ParsingError.TrailingOperatorsInSelector` and `ParsingError.InvalidCharactersInSelector` the causing character is now included in the `Exception.Message`.
* If you have issues formatting HTML with CSS and/or JavaScript included, please read the bullet-proof [How-to in the Wiki](https://github.com/axuno/SmartFormat/wiki/HTML-with-CSS-or-JavaScript)

v2.6.2
===
* Fix: Fully implemented all `Settings.ParseErrorAction`, see [#143](https://github.com/axuno/SmartFormat/pull/143) - Thanks to [Anders Jonsson](https://github.com/andersjonsson)

v2.6.1
===
* Fixed [#136](https://github.com/axuno/SmartFormat/issues/136)
* Upgraded test project to netcoreapp3.1
* Enhanced SubString extension as described in [PR142](https://github.com/axuno/SmartFormat/pull/142) - Thanks to [Anders Jonsson](https://github.com/andersjonsson)

v2.6.0
===
* Migrated project with Nullable Reference Types (NRT) enabled

v2.5.3.0
===
Bugfix: ```ListFormatter``` will now process ```IList``` data sources only.

v2.5.2.0
===
Supported frameworks now are: 
  * .Net Framework 4.6.1, 4.6.2, 4.7.2 and 4.8 (```System.Text.Json``` is not supported for .Net Framework 4.5.x and thus had to be dropped)
  * .Net Standard 2.0 and 2.1

v2.5.1.0
===
* Added ```System.Text.Json.JsonElement``` to the JsonSource extension. ```Newtonsoft.Json``` is still included.
* Added a demo version as a netcoreapp3.1 WindowsDesktop App
* Supported framworks now are: 
  * .Net Framework 4.6.2, 4.7.2 and 4.8 (```System.Text.Json``` is not supported for .Net Framework 4.5.x and thus had to be dropped)
  * .Net Standard 2.0 and 2.1
* Updated the [Wiki](https://github.com/axuno/SmartFormat/wiki)

v2.5.0.0
===

**Sources**
* *New:* Added ```ValueTupleSource``` for ```ValueTuple```s
* *Changed:* ```SmartObjects``` and ```SmartObjectsSource``` are depreciated in favor of ```ValueTupleSource```

**Settings**
* *Breaking Change:* Internal string comparisons (i.e. for placeholder names) are no more culture-specific, but ```Ordinal``` or ```OrdinalIgnoreCase``` respectively. See discussion [under this issue](https://github.com/axuno/SmartFormat/issues/122).
* *Breaking Change:* Default ```ErrorAction``` is now ```ThrowError``` for parser and formatter, instead of ```Ignore```

**Other**
* *Changed:* Removed all members which were flagged obsolete since more than a year.

v2.4.2.0
===

Fixed an [issue](https://github.com/axuno/SmartFormat.NET/issues/116) with SmartObjects

v2.4.0.0 and v2.4.1.0
===

**TimeFormatter**
* *New:* Supports DateTimeOffset as parameter
* *Changed in v2.4.1:* ```DateTime``` operations always use their Universal Time representation. (Before, in case a ```DateTime``` had property ```Kind``` set to ```DateTimeKind.Unspecified```, the result of a comparison was ambiguous.)
* CTOR TimeFormatter(languageCode) throws for not implemented languageCode
* CTOR TimeFormatter() is obsolete (redundant)
* Obsolete in TimeSpanUtility
  * TimeSpan extension method Floor (redundant)
  * TimeSpan extension method Ceiling (redundant)

**ConditionalFormatter**
* *New:* Supports DateTimeOffset as parameter
* *Changed:* ```DateTime``` operations always use their Universal Time representation. (Before, in case a ```DateTime``` had property ```Kind``` set to ```Kind.Unspecified```, the result of a comparison was ambiguous.)

**Demo**
* Updated with DateTimeOffset example
* Updated with TimeFormatter example

v2.3.1.0
===
* Added **SubStringFormatter** [thanks to arilani](https://github.com/axuno/SmartFormat.NET/issues/80)
* Improved code coverage in unit tests
* Updated dependencies
* As announced: **Dropped support for .NET 4.0**, which was released back in 2010.

v2.3.0.0
===
* Support for JSON Objects as data source
* Added **IsMatchFormatter** [thanks to ericpyle](https://github.com/axuno/SmartFormat.NET/issues/88)
* Fixes issue with unsigned integers [#101](https://github.com/axuno/SmartFormat.NET/issues/108)
* **This is the last version supporting .NET 4.0**, which was released back in 2010.

v2.2.0.0
===
* Fixes issue [#101](https://github.com/axuno/SmartFormat.NET/issues/101)
* This version includes a breaking change:
   * Before: ```OnParsingFailure``` event was invoked after each parsing error
   * **Now**: ```OnParsingFailure``` event is invoked after parsing is completed
```ParsingErrorEventArgs``` has a different signature. It now includes **all** ```ParsingErrors``` **with all details** that would be supplied during a parser exception. This also includes messages indicating where there the parse error occurred.


v2.1.0.2
===
* Fixes issue [#94](https://github.com/axuno/SmartFormat.NET/issues/94)

v2.1.0.1
===

* [Characters Literals in Format Strings](https://github.com/axuno/SmartFormat.NET/wiki/Character-Literals-in-Format-Strings)
* [Improved working with several data sources](https://github.com/axuno/SmartFormat.NET/wiki/Several-Data-Sources): SmartObjects
* [Changes in SmartSettings](https://github.com/axuno/SmartFormat.NET/wiki/SmartSettings)
* Fixed signing assemblies

v2.0.0
====
* ReflectionSource now also gets members from base classes
* Added nesting and list tests
* Added coding samples
* Improved source xml docs
* Extended Wiki documentation for error handling and common pitfalls

v1.7.1.1
====
* No more ambiguity between named formatters and string.Format formatting arguments: The parser checks whether the parsed name exists in one of the formatter extensions. 
* SmartFormatter and Parser default to ErrorAction = ErrorAction.Ignore in release AND debug mode
* SmartFormatter has EventHandler OnFormattingFailure
* Parser has EventHandler OnParsingFailure
* Obsolete FormatItem.Text property removed (was replaced by RawText property some time ago)
* Assemblies are signed with a strong name key file
* Supported frameworks are .Net 4.0, .Net 4.5, .Net Core - dropped .Net 3.5 and earlier
* Added an icon to the nuget package

v1.6.1
====
- Fixed issue with parsing named formatter options (#45)
- Minor changes to the Extensions API:
 - Added fields to `ISelectorInfo`
 - Made `Text` obsolete in favor of `RawText`.

v1.6
====
- Added `TemplateFormatter`.
  Allows you to register named templates, to be used within other templates.  
  Note: the TemplateFormatter extension is not a default extension, and must be added manually.

v1.5
====
- Added "Nested Scopes" feature.  This allows a nested template to
  easily access outer scopes, without tricky workarounds.  
  For example: in `"{Person: {Address: {City} {FirstName} } }"`, 
  `{City}` will come from `Address`,
  but `{FirstName}` will come from `Person`.  
  This will work especially well with conditional formatting!
  

v1.4
====
- Massive improvements to the Extension API.  Code is cleaner and easier to use.
- Breaking changes: Any custom Extensions will need to be updated to match the new API.  
  Long story short, instead of 5 parameters passed to `IFormatter.EvaluateFormat`, 
  it now just gets a single `FormatterInfo` argument that contains all parameters.
- Added the "choose" formatter, to eventually replace the ConditionalFormatter.  
  Hopefully it's self explanatory with these usage examples:  
  `"{0:choose(1|2|3):one|two|three|default}"` works like a switch statement
  `"{0:choose(null):nothing|{something}}"` can do null checking
  `"{Gender:choose(Male|Female):his|her}"` works great with Enums


v1.3
====
- Added "Named Formatters", which allows you to use a
  specific formatter by specifying its name.  
  For example, "{0:plural: ___ }" will use the Plural formatter,
  and "{0:default: ___ }" will use the default formatter.
- Added "Formatter Options", which allows you to specify options
  for a named formatter.  This will be used in the near future.  
  For example, "{0:name(options): ___ }"

v1.2
====
- Added .NET v3.5 and .NET v4.0 builds
- Added "releases" folder to hold official releases
- Added `UseAlternativeBraces` method, so that templates can use alternative characters
- Added `SmartFormatter.GetFormatterExtension` and `GetSourceExtension` methods, 
  which can be used to configure extensions
- Added `DefaultTwoLetterISOLanguageName` properties to the `TimeSpanFormatter` and `PluralLocalizationFormatter`
- Fixed `AddExtensions` to insert new extensions before existing ones (otherwise, it's pretty useless) (#17)

V1.1
====
- [#11](#11) Added case insensitivity as option.
- [#12](#12) Added support for expando objects.

[#11]: https://github.com/axuno/SmartFormat.NET/pull/11
[#12]: https://github.com/axuno/SmartFormat.NET/pull/12

V1.0
====
Converted from "CustomFormat" (VB.NET) 
