# AnyConfig

[![nuget](https://img.shields.io/nuget/v/Any-Config.svg)](https://www.nuget.org/packages/Any-Config/)
[![nuget](https://img.shields.io/nuget/dt/Any-Config.svg)](https://www.nuget.org/packages/Any-Config/)
[![Build status](https://ci.appveyor.com/api/projects/status/gfwjabg1pta7em94?svg=true)](https://ci.appveyor.com/project/MichaelBrown/AnyConfig)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/d708cfcc471f415b950cfd27e1829dd9)](https://www.codacy.com/manual/replaysMike/AnyConfig?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=replaysMike/AnyConfig&amp;utm_campaign=Badge_Grade)
[![Codacy Badge](https://api.codacy.com/project/badge/Coverage/d708cfcc471f415b950cfd27e1829dd9)](https://www.codacy.com/app/replaysMike/Any-Config?utm_source=github.com&utm_medium=referral&utm_content=replaysMike/AnySerializer&utm_campaign=Badge_Coverage)

A .net configuration library to make configuration of multi-target applications easier.

## Description

AnyConfig makes configuration on solutions which mix .Net Core and .Net Framework projects easier. It abstracts away ConfigurationManager and IConfiguration loading with no dependencies on Microsoft implementation. You can instead use ConfigurationManager to load either json or xml configuration files, as well as the IConfiguration interface. This allows you to upgrade to json configuration files even for older projects without changing any code!

## Installation
```Powershell
PM> Install-Package Any-Config
```

## Features

* Backwards compatible interface for ConfigurationManager for xml and json
* Supports IConfiguration interface for xml and json
* Supports generics for simple configuration value lookups
* Automatic discovery of configuration files for .Net Core or .Net Framework projects

## Usage

Simplest usage is using the dedicated generics interface:
```csharp
var isEnabled = Config.Get<bool>("IsEnabled");
```

You can also bind your own configuration class:
```csharp
var testConfiguration = Config.Get<MyTestConfiguration>();
```

Grab an IConfiguration for .net core without any Microsoft extensions:
```csharp
var config = Config.GetConfiguration();
var testConfiguration = config.Get<MyTestConfiguration>();
```

If you need, use the legacy ConfigurationManager:
```csharp
var isEnabled = ConfigurationManager.AppSettings["IsEnabled"];
```

The built-in ConfigurationManager supports generics:
```csharp
var isEnabled = ConfigurationManager.AppSettings["IsEnabled"].As<bool>();
```

It also supports reading from json:
```csharp
ConfigurationManager.ConfigurationFilename = "appsettings.json";
var isEnabled = ConfigurationManager.AppSettings["IsEnabled"].As<bool>();
// appsettings.json
{
  "IsEnabled": true
}
```

## Advanced Usage

See the [wiki](https://github.com/replaysMike/AnyConfig/wiki)


