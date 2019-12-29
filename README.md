<h1 align="center">
ExifToolAsync
<br/>
</h1>
<div align="center">

[![Build Status](https://dev.azure.com/cmunckhof/Imaging/_apis/build/status/ExifToolAsync?branchName=develop)](https://dev.azure.com/cmunckhof/Imaging/_build/latest?definitionId=4&branchName=develop)  [![codecov](https://codecov.io/gh/coenm/ExifToolAsync/branch/develop/graph/badge.svg)](https://codecov.io/gh/coenm/ExifToolAsync) 

</div>


This library is an async wrapper around ExifTool. The ExifTool process is started using the `--stay-open` flag. 


## What is ExifTool

According to [exiftool.org](https://exiftool.org/)

> ExifTool is a platform-independent Perl library plus a command-line application for reading, writing and editing meta information in a wide variety of files. ExifTool supports many different metadata formats including EXIF, GPS, IPTC, XMP, JFIF, GeoTIFF, ICC Profile, Photoshop IRB, FlashPix, AFCP and ID3, as well as the maker notes of many digital cameras by Canon, Casio, DJI, FLIR, FujiFilm, GE, GoPro, HP, JVC/Victor, Kodak, Leaf, Minolta/Konica-Minolta, Motorola, Nikon, Nintendo, Olympus/Epson, Panasonic/Leica, Pentax/Asahi, Phase One, Reconyx, Ricoh, Samsung, Sanyo, Sigma/Foveon and Sony.


## API

AsyncExifTool requires an configuration. 

```csharp
// we need to tell AsyncExifTool where  exiftool executable is located.
var exifToolPath = @"D:\exiftool.exe";

// What encoding should AsyncExifTool use to decode the resulting bytes
var exifToolResultEncoding = Encoding.UTF8;

// The newline characters used. Windows is '\r\n', otherwise '\n'.
var exifToolResultNewLine = "\r\n";

// Construction of the ExifToolConfiguration
var config = new AsyncExifToolConfiguration(exifToolPath, exifToolResultEncoding, exifToolResultNewLine);
```

Use the configuration to create an instance of AsyncExifTool.

```csharp
var asyncExifTool = new AsyncExifTool(config);

// to make asyncExifTool operational, we need to initialize.
asyncExifTool.Initialize();

// Define cancellation token to make it possible to cancel an exiftool request if it is not already passed to exiftool.
// Otherwise, cancelling is not possible at this moment.
var ct = new CancellationToken.None;

// from this moment on, asyncExifTool accepts exiftool commands.
// ie.
// get exiftool version
var result1 = await asyncExifTool.ExecuteAsync(new [] { "-ver" }, ct);

// Get ImageSize and ExposureTime tag names and values.
var result2 = await asyncExifTool.ExecuteAsync(new [] { "-s", "-ImageSize", "-ExposureTime", "D:\image1.jpg" } /* cancellation token is optional */);
 
// requests are queued and processed one at a time while keeping exiftool 'open'. 
var task1 = asyncExifTool.ExecuteAsync( .. );
var task2 = asyncExifTool.ExecuteAsync( .. );
var task3 = asyncExifTool.ExecuteAsync( .. );


// Disposing AsyncExifTool
// At this moment, AsyncExifTool does NOT implement IDisposable, or IAsyncDisposable (netstandard2.1) so you will have to Dispose the instance yourself.
// This will cancel all pending requests. The executing request is awaited unless timedout. 
// ExifTool is closed and cannot be initialized anymore nor does it accept any requests.
await asyncExifTool.DisposeAsync(ct);
```
