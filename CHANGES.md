Latest Changes
====

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
