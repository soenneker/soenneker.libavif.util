[![](https://img.shields.io/nuget/v/soenneker.libavif.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libavif.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libavif.util/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.libavif.util/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libavif.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.libavif.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.libavif.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libavif.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libavif.util/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.libavif.util/actions/workflows/codeql.yml)

# Soenneker.Libavif.Util

### A cross-platform .NET API for encoding AVIF images with the official libavif command-line tools.

Encode JPEG, PNG, or Y4M files as AVIF without installing `avifenc` on the host. The required Windows and Linux binaries are included and selected automatically at runtime.

## Quick start

Install the package:

```bash
dotnet add package Soenneker.Libavif.Util
```

Register the utility and encode an image:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Libavif.Util.Options;
using Soenneker.Libavif.Util.Registrars;

await using ServiceProvider provider = new ServiceCollection()
    .AddLogging()
    .AddLibavifUtilAsSingleton()
    .BuildServiceProvider();

ILibavifUtil libavif = provider.GetRequiredService<ILibavifUtil>();

await libavif.Encode("images/photo.jpg", "images/photo.avif", new AvifEncodeOptions
{
    Quality = 80,
    Speed = 6,
    Progressive = true
});
```

That is all the setup required.

## Progressive AVIF

Set `Progressive` to `true` to ask libavif for layered progressive encoding:

```csharp
await libavif.Encode("photo.png", "photo.avif", new AvifEncodeOptions
{
    Progressive = true
});
```

A compatible decoder can display the initial layer before decoding the refinements. Decoder and browser support determines how the image is presented to the user.

## Encoding options

`AvifEncodeOptions` provides the commonly used `avifenc` controls:

| Option | Default | Description |
| --- | ---: | --- |
| `Quality` | `80` | Color quality from `0` through `100`. |
| `AlphaQuality` | `Quality` | Optional alpha-channel quality from `0` through `100`. |
| `Speed` | `6` | Encoder speed from `0` (slowest) through `10` (fastest). |
| `Lossless` | `false` | Enables lossless encoding. |
| `Progressive` | `false` | Enables layered progressive encoding. |
| `StripMetadata` | `true` | Removes EXIF, XMP, and ICC metadata. |

### High quality

```csharp
await libavif.Encode("photo.png", "photo.avif", new AvifEncodeOptions
{
    Quality = 90,
    AlphaQuality = 100,
    Speed = 4
});
```

### Lossless

```csharp
await libavif.Encode("artwork.png", "artwork.avif", new AvifEncodeOptions
{
    Lossless = true,
    Speed = 6
});
```

### Preserve metadata

```csharp
await libavif.Encode("photo.jpg", "photo.avif", new AvifEncodeOptions
{
    StripMetadata = false
});
```

## Use additional avifenc options

For options without a dedicated property, build a structured command. Values and paths are quoted safely.

```csharp
using Soenneker.Libavif.Util.Commands;
using Soenneker.Libavif.Util.Commands.Abstract;

ILibavifCommand command = new AvifCommand()
    .AddFlag("progressive")
    .AddOption("speed", 6)
    .AddArgument("images/source photo.png")
    .AddArgument("images/result photo.avif");

IReadOnlyList<string> output = await libavif.Execute(command, log: false);
```

Prefer `Execute` over raw argument strings when values contain paths or application-supplied input. `Run` remains available when direct command-line control is necessary:

```csharp
IReadOnlyList<string> output = await libavif.Run("--version", log: false);
string version = await libavif.GetVersion();
```

## Dependency injection lifetimes

Both standard lifetimes are available:

```csharp
services.AddLibavifUtilAsSingleton();
services.AddLibavifUtilAsScoped();
```

Register only the lifetime used by the application.

## Supported environments

| Operating system | Architecture | Bundled tool |
| --- | --- | --- |
| Windows | x64 | `avifenc.exe` |
| Linux | x64 | `avifenc` |

Other operating systems and architectures throw `PlatformNotSupportedException`.

## Useful behavior

- Accepts `.jpg`, `.jpeg`, `.png`, and `.y4m` inputs.
- Requires the output path to use the `.avif` extension.
- Creates missing output directories automatically.
- Writes to a unique temporary file and replaces the destination only after encoding succeeds.
- Supports paths containing spaces and quotes.
- Cleans up temporary output after failures or cancellation.
- Accepts a cancellation token on every asynchronous operation.
- Throws `FileNotFoundException` for a missing input or bundled encoder.
- Validates quality and speed values before starting libavif.

The implementation follows the standard Soenneker utility stack for runtime detection, filesystem access, temporary paths, process execution, and pooled command construction.
