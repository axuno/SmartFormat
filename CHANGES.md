Latest Changes
====

What's new in v3.0.0-alpha.2
====

### 1. Significant boost in performance
After implementing a zero allocation `ValueStringBuilder` based on [ZString](https://github.com/Cysharp/ZString) with [#193](https://github.com/axuno/SmartFormat/pull/193):
   * Parsing is 10% faster with 50-80% less GC and memory allocation
   * Formatting is up to 40% faster with with 50% less GC and memory allocation

More optimizations:
   * Added type cache to `ReflectionSource` which increases speed by factor 4. Thanks to [@karljj1](https://github.com/karljj1). ([#155](https://github.com/axuno/SmartFormat/pull/155)).
   * Optimized `DictionarySource` increases speed by 10% with less GC pressure ([#189](https://github.com/axuno/SmartFormat/pull/189))

### 2. Exact control of whitespace text output
This was an issue in v2 and was going back to combining `string.Format` compatibility with *Smart.Format* features. This is resolved by setting the desired mode with `SmartSettings.StringFormatCompatibility` (defaults to `false`). ([#172](https://github.com/axuno/SmartFormat/pull/172))

### 3. Literals may contain any Unicode characters ([#166](https://github.com/axuno/SmartFormat/pull/166))
Add unicode escape characters like `"\u1234"`. Thanks to [@karljj1](https://github.com/karljj1).

### 4. Separate modes for "*Smart.Format* features" and "`string.Format` compatibility"

The mode can be set with `SmartSettings.StringFormatCompatibility`. By default, `SmartSettings.StringFormatCompatibility` is `false`. ([#173](https://github.com/axuno/SmartFormat/pull/173), [#175](https://github.com/axuno/SmartFormat/pull/175))

**a) *Smart.Format* features mode**
   * Brings the full set of features implemented in *Smart.Format*
   * Curly braces are escaped the *Smart.Format* way with `\{` and `\}`.
   * As long as special characters `(){}:\` are escaped with `\`, any character is allowed anywhere. Note: This applies also for the colon.

**b) `string.Format` compatibility mode**
   * SmartFormat acts as a drop-in replacement
   * On top, it allows for named placeholders besides indexed placeholders.
   * The `Parser` will not include the formatter name or formatting options. Like with `string.Format`, everything after the `Selector` separator (colon) is considered as format specifier.
   * Curly braces are escaped the `string.Format` way with `{{` and `}}`. This is the reason for all limitations in `string.Format` compatibility mode
   * `DefaultFormatter` is the only formatter which will be invoked. `null` will be output as `string.Empty`.

### 5. Unlimited characters in formatter options ([#164](https://github.com/axuno/SmartFormat/pull/164), [#165](https://github.com/axuno/SmartFormat/pull/165))

This was a big limitation in v2. In v3, the `Parser` can parse any character as part of formatter options. This means e.g. no limitations for `RegEx` expressions used in `IsMatchFormatter`. Note: Special characters like `(){}:\` must be escaped with `\`.

Say we want to include in the output, whether an email is valid (simplified):
```CSharp
var emailRegEx = "^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*[;]{0,1}\s*)+$";
// ask a little helper to escape the RegEx
var escaped = EscapedLiteral.EscapeCharLiterals('\\', emailRegEx, 0, emailRegEx.Length, true);
// insert the escaped literal from the helper
var result = Smart.Format("Email {0:ismatch("^\(\(\\w+\([-+.]\\w+\)*@\\w+\([-.]\\w+\)*\\.\\w+\([-.]\\w+\)*\)\\s*[;]\{0,1\}\\s*\)+$"):{} is valid|{} NOT valid}", "joe@specimen.com");
// returns "Email joe@specimen.com is valid"
```

### 6. Simplified caching of parser result ([#183](https://github.com/axuno/SmartFormat/pull/183))
"Parse once, format often" is simplified: The `Parser`'s `Format` result can be directly used as a parameter of the `SmartFormatter`.
```Csharp
var temperatures = new[] {-20, -10, -15};
// parse once
var parsedFormat = new Parser().ParseFormat("Temperature is {Temp}°.");
// one SmartFormatter instance
var formatter = Smart.CreateDefaultSmartFormat();
foreach (var current in temperatures)
{
    var result = formatter.Format(parsedFormat, new {Temp = current});
}
```
This pattern is **much** faster than calling
```CSharp
// combined parsing and formatting
var result = Smart.Format("Temperature at North Pole is {Temp}°.", new {Temp = current});
```
in the *foreach* loop.

Class `FormatCache` has been removed in v3.

### 7. Global support for Alignment ([#174](https://github.com/axuno/SmartFormat/pull/174))
In v2, Alignment of output values was limited to the `DefaultFormatter`. It's about the equivalent to e.g. `string.Format("{0,10}")`. 

* Alignment is now supported by all `IFormatter`s.
* Introduced `FormatterSettings.AlignmentFillCharacter`, to customize the the fill character. Default is space (0x20), like with `string.Format`.
* Modified `ListFormatter` so that items can be aligned (but the spacers stay untouched).

### 8. Added `StringSource` as another `ISource` ([#178](https://github.com/axuno/SmartFormat/pull/178))
The `StringSource` takes over functionality, which have been implemented in `ReflectionSource` in v2. Compared to reflection caching, speed is 15% better and has 10% less memory allocation.

`StringSource` brings the following built-in methods (as selector names):
* Length
* ToUpper
* ToUpperInvariant
* ToLower
* ToLowerInvariant
* Trim
* TrimStart
* TrimEnd
* ToCharArray
* Capitalize
* CapitalizeWords
* FromBase64
* ToBase64

All these selector names may be linked. Example with an indexed placeholder:
```CSharp
Smart.Format("{0.ToLower.TrimStart.TrimEnd.ToBase64}", " ABCDE ");
// result: "YWJjZGU="
```

### 9. Introduced Nullable Notation ([#176](https://github.com/axuno/SmartFormat/pull/176))

C# like `nullable` notation allows to display `Nullable<T>` types.

The *Smart.Format* notation is `"{SomeNullable?.Property}"`. If `SomeNullable` is null, the expression is evaluated as `string.Empty`.

The nullable operator can also be used for evaluating a list index. E.g.: `Smart.Format("{TheList?[1]}")` will output `string.Empty`, if the list variable is null.

**Note:** Trying to evaluate `null` without the nullable operator will result in a formatting exception. This is the same behavior as in v2.

All `Format()` methods accept nullable args (**[#196](https://github.com/axuno/SmartFormat/pull/196)**).
Opposed to `string.Format` null(able) arguments are allowed.


### 10. Added `NullFormatter` ([#176](https://github.com/axuno/SmartFormat/pull/176))

In the context of Nullable Notation, the `NullFormatter` has been added. It outputs a custom string literal, if the variable is `null`, else `string.Empty`.

Example:
```CSharp
Smart.Format("{TheValue:isnull:This value is null}", new {TheValue = null});
// Result: "The value is null"
```

### 11. Improved custom `ISource` and `IFormatter` implementations ([#180](https://github.com/axuno/SmartFormat/pull/180))
Any custom exensions can implement `IInitializer`. Then, the `SmartFormatter` will call `Initialize(SmartFormatter smartFormatter)` of the extension, before adding it to the extension list.

### 12. `IFormatter`s have one single, unique name  ([#185](https://github.com/axuno/SmartFormat/pull/185))
In v2, `IFormatter`s could have an unlimited number of names. 
To improve performance, in v3, this is limited to one single, unique name.

### 13. JSON support ([#177](https://github.com/axuno/SmartFormat/pull/177))

Separation of `JsonSource` into 2 `ISource` extensions:
* `NewtonSoftJsonSource`
* `SystemTextJsonSource`

### 14. `SmartFormatter` takes `IList<object>` parameters
Added support for `IList<object>` parameters to the `SmartFormatter` (thanks to [@karljj1](https://github.com/karljj1)) ([#154](https://github.com/axuno/SmartFormat/pull/154))

### 15. `SmartObjects` have been removed
* Removed obsolete `SmartObjects` (which have been replaced by `ValueTuple`) ([`092b7b1`](https://github.com/axuno/SmartFormat/commit/092b7b1b5873301bdfeb2b62f221f936efc81430))

### 16. Bugfix for plural rule ([#182](https://github.com/axuno/SmartFormat/pull/182))
* Fixes #179 (DualFromZeroToTwo plural rule). Thanks to @OhSoGood

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
