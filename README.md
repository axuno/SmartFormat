<img src="https://raw.githubusercontent.com/scottrippey/SmartFormat.NET/main/SmartFormat_64x64.png" width="64" alt="Logo">

[![GitHub release](https://img.shields.io/github/release/axuno/smartformat.net.svg)](https://github.com/axuno/SmartFormat.Net/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/axuno/SmartFormat.Net/blob/main/License.txt)
[![AppVeyor build status windows](https://img.shields.io/appveyor/job/build/axuno/smartformat/windows/version/v3.0?label=windows%20build)](https://ci.appveyor.com/project/axuno/smartformat/branch/version/v3.0)
[![AppVeyor build status linux](https://img.shields.io/appveyor/job/build/axuno/smartformat/linux/version/v3.0?label=linux%20build)](https://ci.appveyor.com/project/axuno/smartformat/branch/version/v3.0)

[![AppVeyor tests](https://img.shields.io/appveyor/tests/axuno/SmartFormat.svg)](https://ci.appveyor.com/project/axuno/SmartFormat/branch/main/tests)
[![codecov](https://codecov.io/gh/axuno/SmartFormat/branch/main/graph/badge.svg)](https://codecov.io/gh/axuno/SmartFormat)

[![Paypal-Donations](https://img.shields.io/badge/Donate-PayPal-important.svg?style=flat-square)](https://www.paypal.com/donate?hosted_button_id=KSC3LRAR26AHN)

**SmartFormat** is a is a **lightweight text templating** library written in C# which can be a drop-in replacement for `string.Format`. More than that **SmartFormat** can format data with named placeholders, lists, localization, pluralization and other smart extensions.

* High performance with low memory footprint
* Minimal, intuitive syntax
* Formatting takes place exclusively at runtime
* Exact control of whitespace text output
* `string.Format` compatibility mode and *SmartFormat* enhanced mode
* Most common data sources work out-of-the-box
* Many built-in formatting extensions
* Custom formatting and source extensions are easy to integrate

### Supported Frameworks
* .Net Framework 4.6.1 and later
* .Net Standard 2.0
* .Net Standard 2.1 and later for best optimizations
 
### Get started
[![NuGet](https://img.shields.io/nuget/v/SmartFormat.svg)](https://www.nuget.org/packages/SmartFormat.Net/) Install the **full** NuGet package *-or-*

[![NuGet](https://img.shields.io/nuget/v/SmartFormat.svg)](https://www.nuget.org/packages/SmartFormat/) Install the **core** NuGet package

[![Docs](https://img.shields.io/badge/docs-up%20to%20date-brightgreen.svg)](https://github.com/axuno/SmartFormat/wiki)
Have a look at the [SmartFormat Wiki](https://github.com/axuno/SmartFormat/wiki)

See the [changelog](CHANGES.md) for changes.

### License

*SmartFormat* is licensed under [The MIT License (MIT)](LICENSE.md)

The bundled project *SmartFormat.ZString* is Copyright © Cysharp, Inc. [Their software](https://github.com/Cysharp/ZString) 
is licensed under [The MIT License (MIT)](src/SmartFormat.ZString/repo/LICENSE). 
Their conversion methods under the `ZString/Number` directory 
is Copyright © .NET Foundation and Contributors und is licensed
under [The MIT License (MIT)](https://github.com/dotnet/runtime/blob/master/LICENSE.TXT).

