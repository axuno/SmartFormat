### Dev Diary for `version/v3.0` branch:

#### Added ValueStringBuilder support ([#193](https://github.com/axuno/SmartFormat/pull/193))

**Significant improvements of performance:**

BenchmarkDotNet performance tests for formatters and `ISource`s now show (depending on different input format strings) the following improvements compared to v2.7.0:
* **increase in speed by up to 40%**
* **decrease of GC pressure** (collects are only GC Generation 0, **allocated memory reduced by up to 60%**)

Formatting measured with a cached parsed `Format`, and including the result `string` returned to the caller. `Parser` was already optimized with PR [#187](https://github.com/axuno/SmartFormat/pull/187). See details in performance tests:

v3.0.0-alpha.1:
* [Parser performance](https://github.com/axuno/SmartFormat/blob/version/v3.0/src/Performance/ParserTests.cs)
* [Formatting performance](https://github.com/axuno/SmartFormat/blob/version/v3.0/src/Performance/FormatTests.cs)
* [Sources performance](https://github.com/axuno/SmartFormat/blob/version/v3.0/src/Performance/SourcePerformanceTests.cs)

v2.7.0:
* [Parser performance](https://github.com/axuno/SmartFormat/blob/version/v3.0/src/Performance_v27/ParserTests.cs)
* [Formatting performance](https://github.com/axuno/SmartFormat/blob/version/v3.0/src/Performance_v27/FormatTests.cs)
* [Sources performance](https://github.com/axuno/SmartFormat/blob/version/v3.0/src/Performance_v27/SourcePerformanceTests.cs)

**Changes in detail:**
* Added [Cysharp/ZString](https://github.com/Cysharp/ZString) as a git subtree to SmartFormat
* Added project `SmartFormat.ZString`
* Replaced all `StringBuilder` implementations with `Utf16ValueStringBuilder`, and all `StringOutput` implementations with `ZStringOutput`
* Included `SmartFormat.ZString` into the nuget package

#### Target frameworks ([#189](https://github.com/axuno/SmartFormat/pull/189))

* Added `netstandard2.1` besides `netstandard2.0` (`net461`+ will use `netstandard2.0` assemblies). `netstandard2.1` provides more built-in support for `ReadOnlySpan<T>` et al.
* Added package `System.Memory`

#### Remove repetitive substring allocations ([#189](https://github.com/axuno/SmartFormat/pull/189))

Connected modifications:

* Added method `Write(ReadOnlySpan<char> text)` to `IFormattingInfo`
* Generated substrings are cached in classes `Format`, `FormatItem`, `LiteralText`, `Placeholder` and `Selector`.
* Evaluating escaped characters for `Placeholder.FormatterOptions` and `LiteralText` work without heap memory allocation.

#### Alignment operator inheritance is optimized ([#189](https://github.com/axuno/SmartFormat/pull/189))

* Alignment implementation introduced with PR [#174](https://github.com/axuno/SmartFormat/pull/174) is modified for better performance
* Added method `Placeholder.AddSelector`
* `Placeholder.Selectors` is now internal. Selectors are accessible with `IReadOnlyList<Selector> Placeholder.GetSelectors()`.

#### DictionarySource performance improved ([#189](https://github.com/axuno/SmartFormat/pull/189))

* Implemented suggestion in issue [#186](https://github.com/axuno/SmartFormat/issues/186) for better speed and less GC pressure.
* Side effect: We're using the `CaseSensitivityType` of the dictionary for getting the value for a key. `Settings.GetCaseSensitivityComparison()` will not be applied.

#### Parser does not allocate any strings ([#187](https://github.com/axuno/SmartFormat/pull/187))

* Reducing GC pressure by avoiding temporary string assignments. Depending on the input string, GC is reduced by 50-80%.
* ParserSettings: All internal character lists are returned as `List<char>`.
* Internal character lists are cached in the parser for better performance
* Connected modifications
  * New performance tests for `Parser`
  * `Placeholder` property `Placeholder?.Parent` is renamed to `Placeholder?ParentPlaceholder` to avoid confusion with `Format` property `Format?.Parent`.
  * `Placeholder` has additional internal properties `FormatterNameStartindex`, `FormatterNameLength`, `FormatterOptionsStartindex` and `FormatterOptionsLength`

#### `IFormatter`s have one single, unique name ([#185](https://github.com/axuno/SmartFormat/pull/185))

* `IFormatter.Names` property is obsolete and replaced with `IFormatter.Name`.
  * If you've been using more than one of the `IFormatter.Names`, you'll have to reduce it to one.
  * If you'd like to use another than the standard name of a formatter, you'll have to set a different name. Example:
  
```CSharp
var smart = Smart.CreateDefaultSmartFormat();
smart.GetFormatterExtension<ChooseFormatter>().Name = "NewName";
smart.GetFormatterExtension<ChooseFormatter>().CanAutoDetect = false;
   ```
* When adding an `IFormatter` extension, it is ensured that the `IFormatter.Name` is not registered already.
* The `IFormatter.Name`'s string case is evaluated as defined in the `SmartSettings.CaseSensitivity` setting.
* `IFormatter.CanAutoDetect` marks such formatters, where `TryEvaluateFormat(IFormattingInfo)` will be called when the format string does *not* contain a formatter name explicitly. This is the same behavior as in v2.x, when `IFormatter.Names` contained the magic `string.Empty` as one of the names. Automatic formatter detection bring few things to keep in mind:
  *  Iterating through the list of formatters brings a performance penalty.
  *  The *first* formatter in the list of format extensions, that is able to process the input format will be invoked. Example: If extensions contain the `ConditionalFormatter` and the `PluralLocalizationFormatter` (in this sequence), in most cases the `ConditionalFormatter` will be invoked implicitly. To use the `PluralLocalizationFormatter`, the format string must contain its formatter name explicitly.
* Calling an `IFormatter` extension explicitly, that cannot process the format, will result in a `FormattingException`. In v2.x, the extension was silently skipped.


#### `Format` replaces `FormatCache` ([#183](https://github.com/axuno/SmartFormat/pull/183))

In v2.x the `FormatCache` class had a `CachedObjects` property, which was not implemented:
```Csharp
public class FormatCache
{
    public FormatCache(Format format)
    { Format = format; }
    public Format Format { get; }
    public Dictionary<string, object> CachedObjects { get; } = new Dictionary<string, object>();
}
```
The class `FormatCache` is now replaced by `Format`. `Format` stores the result from the `Parser` parsing the input string. The `SmartFormatter` has additional overloads for using the cached `Format` as an argument, instead of an additional wrapper around `Format`.

It is **highly recommended** to use these `SmartFormatter` methods whenever there is a **constant input string** with **different data arguments**.
Other changes:

* `SmartSettings` has a public CTOR.
* `SmartFormatter` has a CTOR which takes `SmartSettings` as an optional argument.
* `Smart.CreateDefaultSmartFormat()` takes `SmartSettings` as an optional argument.
* `Parser` has a CTOR which takes `SmartSettings` as an optional argument.

#### Bugfix for plural rule ([#182](https://github.com/axuno/SmartFormat/pull/182))
* Fixes #179 (DualFromZeroToTwo plural rule). Thanks to @OhSoGood

#### Refactored handling of source and formatter extensions ([#180](https://github.com/axuno/SmartFormat/pull/180))

SmartFormatter:
* `SourceExtensions` is a `IReadOnlyList<ISource>`, and can only be manipulated with the methods below. `ISource` instances will be added only once per type.
* `FormatterExtensions` is a `IReadOnlyList<IFormatter>`, and can only be manipulated with the methods below. `IFormatter` instances will be added only once per type. Trying to add a formatter with an existing name will throw.
* Extension can be added and removed using 
  * `AddExtensions(params ISource[] sourceExtensions)`
  * `AddExtensions(int position, params ISource[] sourceExtensions)`
  * `AddExtensions(params IFormatter[] formatterExtensions)`
  * `AddExtensions(int position, params IFormatter[] formatterExtensions)`
  * `RemoveSourceExtension<T>()`
  * `RemoveFormatterExtension<T>()`

Extensions:
* For improved performance it is highly recommended to only add such `ISource` and `IFormatter` extensions that are actually required. `Smart.Format(...)` uses all available extensions as the default. Also, use explicit formatter names instead of letting the `SmartFormatter` implicitly find a matching formatter.
* Neither `ISource` nore `IFormatter` extensions have a CTOR with an argument. This allows for adding extension instances to different `SmartFormatter`s.
* Any exensions can implement `IInitializer`. Then, the `SmartFormatter` will call the method `Initialize(SmartFormatter smartFormatter)` of the extension, before adding it to the extension list.
* The `Source` abstract class implements `IInitializer`. The `SmartFormatter` and the `SmartSettings` are accessible for classes with `Source` as the base class.
* Both, `TimeFormatter` and `TemplateFormatter`, had used the same short name `'t'` up to v2.7.x. `'t'` is removed from `TimeFormatter`.

#### Added `StringSource` as another `ISource` ([#178](https://github.com/axuno/SmartFormat/pull/178))

`StringSource` adds the following selector names, which have before been implemented with `ReflectionSource`:
* Length
* ToUpper
* ToUpperInvariant
* ToLower
* ToLowerInvariant
* Trim
* TrimStart
* TrimEnd
* ToCharArray

Additionally, the following selector names are implemented:
* Capitalize
* CapitalizeWords
* FromBase64
* ToBase64

All these selector names may be linked. Example with indexed placeholders:
```CSharp
Smart.Format("{0.ToLower.TrimStart.TrimEnd.ToBase64}", " ABCDE ");
// result: "YWJjZGU="t
```
This also works for named placeholders.

**Note**: `ReflectionSource` does not evaluate `string`s any more.

The `StringSource` provides additional funcionality and compared to reflection caching, performance is 15% better.


```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET Core SDK=5.0.202
  [Host]        : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=.NET Core 5.0  Runtime=.NET Core 5.0

|             Method |     N |        Mean |     Error |    StdDev |     Gen 0 | Gen 1 | Gen 2 |   Allocated |
|------------------- |------ |------------:|----------:|----------:|----------:|------:|------:|------------:|
| DirectMemberAccess |  1000 |    261.5 us |   5.13 us |   8.71 us |   20.9961 |     - |     - |   171.88 KB |
| SfWithStringSource |  1000 |  1,792.1 us |   9.97 us |   9.32 us |  167.9688 |     - |     - |  1382.81 KB |
|  SfCacheReflection |  1000 |  2,026.1 us |  20.62 us |  19.29 us |  179.6875 |     - |     - |  1476.56 KB |
|SfNoCacheReflection |  1000 | 13,091.9 us | 129.38 us | 121.02 us |  781.2500 |     - |     - |  6468.75 KB |
|                    |       |             |           |           |           |       |       |             |
| DirectMemberAccess | 10000 |  2,519.2 us |  49.85 us |  53.34 us |  207.0313 |     - |     - |  1718.75 KB |
| SfWithStringSource | 10000 | 17,886.7 us | 151.64 us | 141.84 us | 1687.5000 |     - |     - | 13828.13 KB |
|  SfCacheReflection | 10000 | 20,918.6 us | 166.88 us | 156.10 us | 1781.2500 |     - |     - | 14765.63 KB |
|SfNoCacheReflection | 10000 |130,049.2 us |1,231.06us |1,027.99us | 7750.0000 |     - |     - | 64687.81 KB |
```

#### JSON Source ([#177](https://github.com/axuno/SmartFormat/pull/177))

Separation of `JsonSource` into 2 `ISource` extensions:
* `NewtonSoftJsonSource`
* `SystemTextJsonSource`

#### Introduced Nullable Notation ([#176](https://github.com/axuno/SmartFormat/pull/176))

The C# like `nullable` notation allows to display `Nullable<T>` types, depending on whether a variable contains the underlying type's value or `null`. The Smart.Format notation is `"{SomeNullable?.Property}"`. If `SomeNullable` is null, the expression is evaluated as `string.empty`.

In this context the `NullFormatter` was introduced. It outputs a custom string literal, if the variable is `null`, else `string.Empty`.

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
In the most simple scenario, both evaluations can also be combined with the `ChooseFormatter` (but 35% more slow):
```Csharp
// result: "This value is null" or the string value
var result = smart.Format("{TheValue?:choose(null):This value is null|{}}}", new {TheValue = null});
```
In more complex scenarios, you will, however, use the nullable notation together with other formatter extensions, like a `ListFormatter`.

**Note:** Trying to evaluate `null` without the nullable operator will result in a formatting exception. This is the same behavior as in SmartFormat 2.x.

Changes connected to nullable notation:
* Source extensions should have `Source` as the abstract base class, instead of `ISource`.
* Source extensions should check for `null` values together with the nullable operator and return `true` in the `bool TryEvaluateSelector(ISelectorInfo selectorInfo)` method.
* The `Parser` accepts `?` as another standard operator.
* The nullable operator can also be used for evaluating a list index, e.g. `"{TheList?[1]}"`.

#### Refactored implementation for string alignment ([#174](https://github.com/axuno/SmartFormat/pull/174))
Example: `Smart.Format("{placeholder,15}")` where ",15" is the string alignment

* Aligment is now implemented into class `FormattingInfo`, so it is **always available**. 
* Introduced `FormatterSettings.AlignmentFillCharacter`, to customize the fill character. Default is space (0x20), like with `string.Format`.
* Former implementation in `DefaultFormatter` is removed.
* Modified `ListFormatter` so that items can be aligned, but the spacers stay untouched
* `IFormattingInfo.Alignment` now returns the alignment of the current `Placeholder`, or - if this is null - the `Alignment` of any parent `IFormattingInfo` that is not zero.
* Renamed `IFormattingInfo.Write(Format format, object value)` to `FormatAsChild(Format format, object value)` to make clear that nothing is written to `IOutput` (this happens in a next step).
* Added dedicated AlignmentTests

#### Separated mode for Smart.Format features from `string.Format` compatibility ([#173](https://github.com/axuno/SmartFormat/pull/173), [#175](https://github.com/axuno/SmartFormat/pull/175))
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
   * Even in compatibility mode, *Smart.Format* will
     * Process named `Placeholder`s (beside indexed `Placeholder`s)
     * have `ISource`s available for `Placeholder`s.
     * be able to process escaped string literals like \n, \U1234 etc.

**2. SmartFormat mode:**
  * As long as special characters (`(){}:\`) are escaped, any character is allowed anywhere. Note: This applies also for the colon. Example:
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
* Constrain custom selectors and operators ([#172](https://github.com/axuno/SmartFormat/pull/172))
  * Custom selector chars can be added, if not disallowed or in use as an operator char
  * Custom operator chars can be added, if not disallowed or in use as a selector char
  * Alphanumeric selector chars are the only option now and cannot be degraded to pure numeric chars
  * `PlaceholderBeginChar`, `PlaceholderEndChar`, `FormatterOptionsBeginChar` and `FormatterOptionsEndChar` now only have getters
* Exact control of whitespace text output
* Reduced substring usage with literal text leads to a significant reduction of GC and memory allocation ([#169](https://github.com/axuno/SmartFormat/pull/169)) 
* Add unicode escape characters ([#166](https://github.com/axuno/SmartFormat/pull/166)). Thanks to [@karljj1](https://github.com/karljj1)
* `FormatItem` abstract class ([#167](https://github.com/axuno/SmartFormat/pull/167))
  * Obsoleted public field `baseString`, changed to `BaseString` property
  * Obsoleted public field `startIndex`, changed to `StartIndex` property
  * Obsoleted public field `endIndex`, changed to `IndexIndex` property
  * Added `Length`property (returing `EndIndex` - `StartIndex`)
* Parser can parse any character as part of formatter options. This means e.g. no limitations for `RegEx` expressions used in `IsMatchFormatter`. ([#165](https://github.com/axuno/SmartFormat/pull/165)) 
  * Obsoleted public property `Format.parent`, changed to `Format.Parent`
  * Obsoleted public property `Placeholder.parent`, changed to `Placeholder.Parent`
  * `Placeholder` now has `FormatterOptionsRaw` with original (unescaped) string and `FormatterOptions` with the escaped string.
  * `EscapedLiteral`: 15% faster, 50% less GC
* `Parser` ([#164](https://github.com/axuno/SmartFormat/pull/164)):
  * Moved settings from class `Parser` to class `ParserSettings` and corresponding members in `Parser` are marked as obsolete. Calling `UseAlternativeEscapeChar()` with argument, which is not a backslash, will throw an `ArgumentException`.
  * `Parser.UseAlternativeBraces(..)` is obsolete and not supported any more
  * The only 'alternative' character for escaping curly braces is now `\`, i.e. `\{` and `\}` instead of `{{` and `}}`. The latter are used if `SmartSettings.StringFormatCompatibility` is `true`.
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
