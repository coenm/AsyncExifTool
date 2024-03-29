<h1 align="center">
<img src="https://raw.githubusercontent.com/coenm/AsyncExifTool/develop/icon/AsyncExifTool.512.png" alt="AsyncExifTool" width="256"/>
<br/>
AsyncExifTool
</h1>

<div align="center">

[![Build Status](https://dev.azure.com/cmunckhof/Imaging/_apis/build/status/AsyncExifTool?branchName=develop)](https://dev.azure.com/cmunckhof/Imaging/_build/latest?definitionId=6&branchName=develop) [![codecov](https://codecov.io/gh/coenm/AsyncExifTool/branch/develop/graph/badge.svg)](https://codecov.io/gh/coenm/AsyncExifTool) [![NuGet](https://img.shields.io/nuget/v/CoenM.AsyncExifTool.svg)](https://www.nuget.org/packages/CoenM.AsyncExifTool/)

</div>

This library is an async wrapper around ExifTool. The ExifTool process is started using the `--stay-open` flag.

This library does NOT include an instance of ExifTool. You have to install/compile/unpack ExifTool yourself and point AsyncExifTool to the right location.

## What is ExifTool

According to [exiftool.org](https://exiftool.org/)

> ExifTool is a platform-independent Perl library plus a command-line application for reading, writing and editing meta information in a wide variety of files. ExifTool supports many different metadata formats including EXIF, GPS, IPTC, XMP, JFIF, GeoTIFF, ICC Profile, Photoshop IRB, FlashPix, AFCP and ID3, as well as the maker notes of many digital cameras by Canon, Casio, DJI, FLIR, FujiFilm, GE, GoPro, HP, JVC/Victor, Kodak, Leaf, Minolta/Konica-Minolta, Motorola, Nikon, Nintendo, Olympus/Epson, Panasonic/Leica, Pentax/Asahi, Phase One, Reconyx, Ricoh, Samsung, Sanyo, Sigma/Foveon and Sony.

## Branching model

This project uses [GitFlow](http://nvie.com/posts/a-successful-git-branching-model/) as branching model.

## Dependencies

AsyncExifTool has two external dependencies;

- [MedallionShell](https://www.nuget.org/packages/MedallionShell/) for managing the process to ExifTool;
- Async primitives from the [Nito.AsyncEx](https://www.nuget.org/packages/Nito.AsyncEx/) package.

## Logging

AsyncExifTool has optional logging available. You have to implement the `ILogger` interface yourself and pass it as a constructor parameter to `AsyncExifTool`. See the sample application for a possible implementation using `NLog`.

You *cannot* depend on log messages for further releases as these might change 

## Async Dispose

`IAsyncDispose` is available since `netstandard2.1`. This library also targets `netstandard2.0` and `net461` where `AsyncExifTool` not only implements the older `IDisposable` interface but also has an  extra `AsyncDispose` method available.

You should explicitly call this method when you want to use this.

## API

See [DotNet APIs](http://dotnetapis.com/pkg/CoenM.AsyncExifTool) for the complete API.

AsyncExifTool requires an configuration.

<!-- snippet: ExifToolConfiguration -->
<a id='snippet-exiftoolconfiguration'></a>
```cs
// We need to tell AsyncExifTool where exiftool executable is located.
var exifToolExe = @"D:\exiftool.exe";

// The encoding AsyncExifTool should use to decode the resulting bytes
Encoding exifToolEncoding = Encoding.UTF8;

// common args for each exiftool command.
// see https://exiftool.org/exiftool_pod.html#common_args for more information.
// can be null or empty
var commonArgs = new List<string>
    {
        "-common_args",
    };

// custom exiftool configuration filename.
// see https://exiftool.org/exiftool_pod.html#config-CFGFILE for more information.
// make sure the filename exists.
// it is also possible to create a configuration without a custom exiftool config.
var customExifToolConfigFile = @"C:\AsyncExifTool.ExifTool_config";

// Create configuration to be used in AsyncExifTool.
AsyncExifToolConfiguration asyncExifToolConfiguration = string.IsNullOrWhiteSpace(customExifToolConfigFile)
    ? new AsyncExifToolConfiguration(exifToolExe, exifToolEncoding, commonArgs)
    : new AsyncExifToolConfiguration(exifToolExe, customExifToolConfigFile, exifToolEncoding, commonArgs);
```
<sup><a href='/tests/Samples/Program.cs#L22-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-exiftoolconfiguration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Use the configuration to create an instance of AsyncExifTool.

<!-- snippet: ExifToolExampleUsage -->
<a id='snippet-exiftoolexampleusage'></a>
```cs
var asyncExifTool = new AsyncExifTool(asyncExifToolConfiguration);

// To make asyncExifTool operational, we need to initialize.
// This method can throw an exception
asyncExifTool.Initialize();

// Define cancellation token to make it possible to cancel an exiftool request if it is not already passed to exiftool.
// Otherwise, cancelling is not possible at this moment.
CancellationToken ct = CancellationToken.None;

// From this moment on, asyncExifTool accepts exiftool commands.
// i.e. get exiftool version
var result1 = await asyncExifTool.ExecuteAsync(new[] { "-ver" }, ct);

// Get ImageSize and ExposureTime tag names and values.
// CancellationToken is optional for ExecuteAsync method.
var result2 = await asyncExifTool.ExecuteAsync(new[] { "-s", "-ImageSize", "-ExposureTime", @"D:\image1.jpg" });

// Commands are queued and processed one at a time while keeping exiftool 'open'.
var exifToolCommand = new[] { "-ver" };
Task<string> task1 = asyncExifTool.ExecuteAsync(exifToolCommand, CancellationToken.None);
Task<string> task2 = asyncExifTool.ExecuteAsync(exifToolCommand);
Task<string> task3 = asyncExifTool.ExecuteAsync(exifToolCommand, ct);

// Example writing metadata to image
var result3 = await asyncExifTool.ExecuteAsync(new[] { "-XMP-dc:Subject+=Summer", @"D:\image1.jpg" }, ct);

// Disposing AsyncExifTool
// ExifTool is closed and cannot be initialized anymore nor does it accept any requests.
await asyncExifTool.DisposeAsync();
```
<sup><a href='/tests/Samples/Program.cs#L153-L186' title='Snippet source file'>snippet source</a> | <a href='#snippet-exiftoolexampleusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Icon

[Photo](https://thenounproject.com/term/photo/2013925) designed by [OCHA Visual](https://thenounproject.com/ochavisual) from [The Noun Project](https://thenounproject.com).
