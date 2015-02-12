LATEST CHANGES
====

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

[#11]: https://github.com/scottrippey/SmartFormat.NET/pull/11
[#12]: https://github.com/scottrippey/SmartFormat.NET/pull/12

V1.0
====
Converted from "CustomFormat" (VB.NET) 
