Latest Changes
====

v3.2.1
===

### PluralLocalizationFormatter

* Fix: Auto-detection of PluralLocalizationFormatter does not throw for values not convertible to decimal by @axunonb in https://github.com/axuno/SmartFormat/pull/330  Resolves #329 (Thanks to @BtbN)

    * Current behavior, introduced in v3.2.0:
    When `PluralLocalizationFormatter.CanAutoDetect == true`, values that are not convertible to `decimal` will throw then trying to `IConvertible.ToDecimal(...)`

    * New behavior, equivalent to v3.1.0:
    When `PluralLocalizationFormatter.CanAutoDetect == true`, for values that are not convertible to `decimal`, `IFormatter.TryEvaluateFormat(...)` will return `false`

* Fix processing for Singular languages by @axunonb in https://github.com/axuno/SmartFormat/pull/322

### Other Changes

* EditorConfig and appveyor.yml by @axunonb in https://github.com/axuno/SmartFormat/pull/319
* Integrate Cysharp.ZString release v2.5.0 (26 Oct 2022) by @axunonb in https://github.com/axuno/SmartFormat/pull/323
* Fix: PluralRules for Czech locale by @alexheb in https://github.com/axuno/SmartFormat/pull/325
* Fixes for Demo App and NUnit TestAdapter by @axunonb in https://github.com/axuno/SmartFormat/pull/328

**Full Changelog**: https://github.com/axuno/SmartFormat/compare/v3.2.0...v3.2.1

v3.2.0
===
### Enhancements

* Remove usage of Linq for less GC
* Add `IConvertable` support for `PluralLocalizationFormatter` and `ConditionalFormatter`
* `ListFormatter`
  * ListFormatter handles selector name "Index" in `IEnumerable`s and `IList`s: In `v1.6.1` a Selector was tested for having the name **"index"**, even if data was not an `IList`, and returned the `CollectionIndex`. This is now implemented again in the `ListFormatter.TryEvaluateSelector(...)`
  * Set the `ParentPlaceholder` property for item `Format`s
  * Use `PooledObject<T>` where possible, so objects will be returned to `ObjectPool` also in case of exceptions

### Fixes
* `FormatItem.AsSpan()` returns the correct name
* Remove potential namespace collisions: All public types in namespace `Cysharp.Text` are now internal


v3.1.0
===

This is a feature update, that is released upon feedback from the community.

### Thread-safe Mode

Thread-safe mode is now enabled by default:
`SmartSettings.IsThreadSafeMode == true`.<br/>
This has no impact on the API.

In case *SmartFormat* is *exclusively* utilized in a single-threaded context, `SmartSettings.IsThreadSafeMode=false` should be considered for enhanced performance.

### Static `Smart` Methods for Formatting

Static `Smart` methods like Smart.Format(*format*, *args*) can now be called in an `async` / multi-threaded context.

The `SmartFormatter` instance returned by `Smart.Default` is flagged with the `ThreadStatic` attribute.

### `ListFormatter` may have Placeholders in "spacers"

Thanks to **[karljj1](https://github.com/axuno/SmartFormat/commits?author=karljj1)** for the PR.

Before *v3.1.0* the format options for `ListFormatter` could only contain literal text. Now `Placeholder`s are allowed.

#### Example:

```CSharp
var args = new {
    Names = new[] { "John", "Mary", "Amy" },
    IsAnd = true, // true or false
    Split = ", "  // comma and space as list separator
};
_ = Smart.Format("{Names:list:{}|{Split}| {IsAnd:and|nor} }", args);
// Output for "IsAnd=true":  "John, Mary and Amy"
// Output for "IsAnd=false": "John, Mary nor Amy"
```
<br/>

v3.0.0
===

> You'll find a detailed description for all changes including code samples in the **[Wiki](https://github.com/axuno/SmartFormat/wiki)**.

### Significant boost in performance

After implementing a **zero allocation `ValueStringBuilder`** ([#193](https://github.com/axuno/SmartFormat/pull/193), [#228](https://github.com/axuno/SmartFormat/pull/228)) and **Object Pools** ([#229](https://github.com/axuno/SmartFormat/pull/229)) for all classes which are frequently instantiated:
   * Parsing is 10% faster with 50-80% less GC and memory allocation
   * Formatting is up to 40% faster with 50% less GC and memory allocation

Sample of BenchmarkDotNet results under NetStandard2.1:
```
|         Method |     N |     Mean |   Error |  StdDev |      Gen 0 |     Gen 1 | Gen 2 | Allocated |
|--------------- |------ |---------:|--------:|--------:|-----------:|----------:|------:|----------:|
SmartFormat v2.7.2 (only SingleThread)
| Format         | 10000 | 223.9 ms | 1.48 ms | 1.38 ms | 21333.3333 |         - |     - |    172 MB |
SmartFormat v3.0.0
| SingleThread   | 10000 | 108.2 ms | 0.52 ms | 0.49 ms |  3200.0000 |         - |     - |     26 MB |
| ThreadSafe     | 10000 | 128.0 ms | 1.29 ms | 1.21 ms |  6000.0000 |         - |     - |     48 MB |
```

### string.Format Compatibility ([#172](https://github.com/axuno/SmartFormat/pull/172), [#173](https://github.com/axuno/SmartFormat/pull/173), [#175](https://github.com/axuno/SmartFormat/pull/175))

The main point about `string.Format` compatibility is, how curly braces and colons are processed in the format string.

In most cases `string.Format` compatibility does not bring any advantages.

With `SmartSettings.StringFormatCompatibility = true` *SmartFormat* is fully compatible. The downside, however, ist that custom formatter extensions cannot be parsed and used with this setting.

With `SmartSettings.StringFormatCompatibility = false` (default), all features of *SmartFormat* are available.

Reasoning: The distinction was necessary because of syntax conflicts between *SmartFormat* extensions and `string.Format`. It brings a more concise and clear set of formatting rules and full `string.Format` compatibility even in "edge cases".


### Nullable Notation

* C# like `nullable` notation allows to display `Nullable<T>` types safely ([#176](https://github.com/axuno/SmartFormat/pull/176)).

* The *SmartFormat* notation is `"{SomeNullable?.Property}"`. If `SomeNullable` is null, the expression is evaluated as `string.Empty`. `"{SomeNullable.Property}"` would throw a `FormattingException`.

### Thread-Safe Caching ([#229](https://github.com/axuno/SmartFormat/pull/229))

Object Pools, Reflection and other caches can operate in thread-safe mode.
(`SmartFormatter`s and `Parser`s still require one instance per thread.)

### No Limits for Allowed Characters

* This was a limitation in v2. In v3, the `Parser` can parse any character as part of formatter options. This means e.g. no limitations for `RegEx` expressions used in `IsMatchFormatter`. Note: Special characters like `(){}:\` must be escaped with `\`.

* Literals may contain any Unicode characters ([#166](https://github.com/axuno/SmartFormat/pull/166)). Add unicode escape characters like `"\u1234"`. Thanks to [@karljj1](https://github.com/karljj1).

* Global support for Alignment ([#174](https://github.com/axuno/SmartFormat/pull/174)): In v2, Alignment of output values was limited to the `DefaultFormatter`. It's about the equivalent to e.g. `string.Format("{0,10}")`. Alignment is now supported by all `IFormatter`s.

* Full control about output characters, including whitespace.

### Improved parsing of HTML input

Introduced `bool ParserSettings.ParseInputAsHtml`.
The default is `false`.

If `true`, the`Parser` will parse all content inside &lt;script&gt; and &lt;style&gt; tags as `LiteralText`. All other places may still contain `Placeholder`s.

This is because &lt;script&gt; and &lt;style&gt; tags may contain curly or square braces, that interfere with the *SmartFormat* {`Placeholder`}.

### Improved Extensions

#### Split character for options and formats

The character to split options and formats can be changed. This allows having the default split character `|` as part of the output string.

Affects `ChooseFormatter`, `ConditionalFormatter`, `IsMatchFormatter`, `ListFormatter`, `PluralLocalizationFormatter`, `SubStringFormatter`.

#### `ReflectionSource`
   
Added a type cache which increases speed by factor 4.
Thanks to [@karljj1](https://github.com/karljj1). ([#155](https://github.com/axuno/SmartFormat/pull/155))

#### `DictionarySource`
   
Speed increased by 10% with less GC pressure ([#189](https://github.com/axuno/SmartFormat/pull/189)).

#### `IsMatchFormatter`

The `IsMatchFormatter` is a formatter with evaluation of regular expressions. It allows to control its output depending on a `RegEx` match.

**New:** The formatter can output matching group values of a `RegEx` ([#245](https://github.com/axuno/SmartFormat/pull/245)).

#### `PluralLocalizationFormatter` ([#209](https://github.com/axuno/SmartFormat/pull/209))

* Constructor with string argument for default language is removed.
* Property `DefaultTwoLetterISOLanguageName` is removed.
* `CultureInfo.InvariantCulture` maps to `CultureInfo.GetCultureInfo("en")` ([#243](https://github.com/axuno/SmartFormat/pull/243)).

Culture is now determined in this sequence (same as with `LocalizationFormatter`):<br/>
  a) Get the culture from the `FormattingInfo.FormatterOptions`.<br/>
  b) Get the culture from the `IFormatProvider` argument (which may be a `CultureInfo`) to `SmartFormatter.Format(IFormatProvider, string, object?[])`<br/>
  c) The `CultureInfo.CurrentUICulture`<br/>

#### `TimeFormatter` ([#220](https://github.com/axuno/SmartFormat/pull/220), [#221](https://github.com/axuno/SmartFormat/pull/221), [#234](https://github.com/axuno/SmartFormat/pull/234))

* Constructor with string argument for default language is removed.
* Property `DefaultTwoLetterISOLanguageName` is removed.

Culture is now determined in this sequence (same as with `LocalizationFormatter`):<br/>
  a) Get the culture from the `FormattingInfo.FormatterOptions`.<br/>
  b) Get the culture from the `IFormatProvider` argument (which may be a `CultureInfo`) to `SmartFormatter.Format(IFormatProvider, string, object?[])`<br/>
  c) The `CultureInfo.CurrentUICulture`<br/>

Extended `CommonLanguagesTimeTextInfo`, which now includes French, Spanish, Portuguese, Italian and German as new languages besides English out-of-the-box.

This notation - using formats as formatter options - was allowed in *SmartFormat* *v2.x*, but is now depreciated. It is still detected and working, as long as the format part is left empty.
```CSharp
var formatDepreciated = "{0:time(abbr hours noless)}";
```
This format string is recommended for *SmartFormat* *v3* and later. It allows for including the language as an option to the `TimeFormatter`:
```CSharp
// Without language option:
var formatRecommended = "{0:time:abbr hours noless:}";
// With language option:
var formatRecommended = "{0:time(en):abbr hours noless:}";
```

#### `SubStringFormatter`

The formatter now accecpts a format argument with a nested `Placeholder` that lets you format the result of the sub-string operation ([#258](https://github.com/axuno/SmartFormat/pull/258)).

Example: Convert the sub-string to lower-case:
```CSharp
Smart.Format("{0:substr(0,2):{ToLower}}", "ABC");
```

#### `ChooseFormatter` [#253](https://github.com/axuno/SmartFormat/pull/253)

Modified `ChooseFormatter` case-sensitivity for option strings. This modification is compatible with v2:

* `bool` and `null` as string: always case-insensitive
* using `SmartSettings.CaseSensitivity` unless overridden with `ChooseFormatter.CaseSensitivity`
* option strings comparison is culture-aware


### Added Extensions

#### `StringSource`

The `StringSource` takes over a part of the functionality, which has been implemented in `ReflectionSource` in v2. Compared to reflection **with** caching, speed is 20% better at 25% less memory allocation. ([#178](https://github.com/axuno/SmartFormat/pull/178), [#216](https://github.com/axuno/SmartFormat/pull/216))

#### `KeyValuePairSource` ([#244](https://github.com/axuno/SmartFormat/pull/244))

The `KeyValuePairSource` is a simple, cheap and performant way to create named placeholders.

#### JSON support

Separation of `JsonSource` into 2 `ISource` extensions ([#177](https://github.com/axuno/SmartFormat/pull/177), [#201](https://github.com/axuno/SmartFormat/pull/201)):

* `NewtonSoftJsonSource`
* `SystemTextJsonSource`

#### `PersistentVariableSource` and `GlobalVariableSource`

Both provide global variables that are stored in `VariablesGroup` containers. These variables are not passed in as arguments when formatting a string. Instead, they are taken from one of these two registered (global) `ISource`s. ([#233](https://github.com/axuno/SmartFormat/pull/233))

Credits to [Needle](https://github.com/needle-tools)
   and their [PersistentVariablesSource](https://github.com/needle-mirror/com.unity.localization/blob/master/Runtime/Smart%20Format/Extensions/PersistentVariablesSource.cs) extension to *SmartFormat*.

#### `NullFormatter`

In the context of Nullable Notation (see below), the `NullFormatter` has been added. It outputs a custom string literal, if the variable is `null`, else another literal (default is `string.Empty`) or a nested `Placeholder`. ([#176](https://github.com/axuno/SmartFormat/pull/176), [#199](https://github.com/axuno/SmartFormat/pull/199))

#### `LocalizationFormatter`

  * Added `LocalizationFormatter` to localize literals and placeholders ([#207](https://github.com/axuno/SmartFormat/pull/207)).

  * Added `ILocalizationProvider` and a standard implemention as `LocalizationProvider`, which handles `resx` resource files. A fallback culture can be set. `LocalizationProvider` can search an unlimited number of defined resoures.

### Miscellaneous

#### Formatter Name

`IFormatter`s have one single, unique name ([#185](https://github.com/axuno/SmartFormat/pull/185)).
In v2, `IFormatter`s could have an unlimited number of names. 

#### `IInitializer` Interface

Any (custom) `ISource` and `IFormatter` can implement `IInitializer`. Then, the `SmartFormatter` will call `Initialize(SmartFormatter smartFormatter)` of the extension, before adding it to the extension list ([#180](https://github.com/axuno/SmartFormat/pull/180)).

#### New parameter type for `SmartFormatter.Format(...)`

Added support for `IList<object>` parameters to the `SmartFormatter` (thanks to [@karljj1](https://github.com/karljj1)) ([#154](https://github.com/axuno/SmartFormat/pull/154))

#### `SmartObjects`

Removed obsolete `SmartObjects` (which have been replaced by `ValueTuple`) ([`092b7b1`](https://github.com/axuno/SmartFormat/commit/092b7b1b5873301bdfeb2b62f221f936efc81430))

#### Parsing HTML input

Introduced experimental `bool ParserSettings.ParseInputAsHtml`.
The default is `false`.

If `true`, the`Parser` will parse all content inside &lt;script&gt; and &lt;style&gt; tags as `LiteralText`. This is because &lt;script&gt; and &lt;style&gt; tags may contain curly or square braces, that interfere with the *SmartFormat* {`Placeholder`}.

 All other places may still contain `Placeholder`s. ([#203](https://github.com/axuno/SmartFormat/pull/203))

#### *SmartFormat* Packages

**SmartFormat.NET**

This is a package which references **all packages** below.

**SmartFormat**

SmartFormat is the **core package**. It comes with the most frequently used extensions built-in.

**SmartFormat.Extensions.System.Text.Json**

This package is a SmartFormat extension for formatting `System.Text.Json` types as a source.

**SmartFormat.Extensions.Newtonsoft.Json**

This package is a SmartFormat extension for formatting `Newtonsoft.Json` types as a source.

**SmartFormat.Extensions.Xml**

This package is a SmartFormat extension for reading and formatting `System.Xml.Linq.XElement`s.

**SmartFormat.Extensions.Time**

This package is a SmartFormat extension for formatting `System.DateTime`, `System.DateTimeOffset` and `System.TimeSpan` types.

---
## v2 Releases

---

v2.7.3
===
* **Hot fix**: Newtonsoft.Json prior to version 13.0.1 is vulnerable. The minimum version of the package reference is now 13.0.1
Newtonsoft.Json prior to version 13.0.1 is vulnerable to Insecure Defaults due to improper handling of expressions with high nesting level that lead to StackOverFlow exception or high CPU and RAM usage. Exploiting this vulnerability results in Denial Of Service (DoS).


v2.7.2
===
* **Fixed**: `ConditionalFormatter` processes unsigned numbers in arguments correctly.
* **Fixed**: `JsonSource`: Corrected handling of `null` values in `Newtonsoft.Json` objects.

v2.7.1
===
* **Fixed**: [#179](https://github.com/axuno/SmartFormat/issues/179) DualFromZeroToTwo plural rule. Thanks to [@OhSoGood](https://github.com/OhSoGood)
* **Fixed**: [#211](https://github.com/axuno/SmartFormat/issues/211) Illegal placeholder characters that are not 8-bit, will no more throw unexpected `ThrowByteOverflowException`. Thanks to [@bogatykh](https://github.com/bogatykh)

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
* Added a demo version as a net5.0 WindowsDesktop App
* Supported framworks now are: 
  * .Net Framework 4.6.1, 4.7.2 and 4.8 (```System.Text.Json``` is not supported for .Net Framework 4.5.x and thus had to be dropped)
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
