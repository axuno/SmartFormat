<img src="https://raw.githubusercontent.com/scottrippey/SmartFormat.NET/master/SmartFormat_64x64.png" width="64" alt="Logo">

[![GitHub release](https://img.shields.io/github/release/axuno/smartformat.net.svg)](https://github.com/axuno/SmartFormat.Net/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/axuno/SmartFormat.Net/blob/master/License.txt)
[![Build status](https://ci.appveyor.com/api/projects/status/g27r62fm9c7e0ctv?svg=true)](https://ci.appveyor.com/project/axuno/smartformat)
[![AppVeyor tests](https://img.shields.io/appveyor/tests/axuno/SmartFormat.svg)](https://ci.appveyor.com/project/axuno/SmartFormat/branch/master/tests)
[![codecov](https://codecov.io/gh/axuno/SmartFormat/branch/master/graph/badge.svg)](https://codecov.io/gh/axuno/SmartFormat)

**SmartFormat** is a **string composition** library written in C# which is basically compatible with string.Format. More than that **SmartFormat** can format data with named placeholders, lists, pluralization and other smart extensions.

### Supported Frameworks
* .Net Framework 4.6.1, 4.6.2, 4.7.2 and 4.8
* .Net Standard 2.0 and 2.1

### Get started
[![NuGet](https://img.shields.io/nuget/v/SmartFormat.Net.svg)](https://www.nuget.org/packages/SmartFormat.Net/) Install the NuGet package

[![Docs](https://img.shields.io/badge/docs-up%20to%20date-brightgreen.svg)](https://github.com/axuno/SmartFormat.Net/wiki)
Have a look at the [SmartFormat.Net Wiki](https://github.com/axuno/SmartFormat.Net/wiki)

See [changelog](CHANGES.md) for changes.

# Version 3.0
We have started to think about a new version of ```SmartFormat.Net``` and **would like to collect your input using [GitHub Discussions](https://github.com/axuno/SmartFormat/discussions/139)**.

* Improve management of extensions for better performance:
  * make named formatters obligatory instead of iterating through all formatters
  * the sequence, how extensions are loaded should not have an impact on identifying the right formatter
* Make caching of ```Parser.ParseFormat``` results the standard behavior
* Support for Net 5.0
* Remove ```public``` properties/methods which should better be ```internal``` or even ```privat```
* Upgrade the project to C# 8 with nullable reference types included
* Code clean-up: Make use of current C# features, add missing comments
* Re-organize unit tests
* ... ?
