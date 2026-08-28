[![](https://img.shields.io/nuget/v/soenneker.libavif.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libavif.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libavif.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.libavif.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.libavif.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libavif.util/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Libavif.Util
### A structured, cross-platform .NET API for AVIF encoding with bundled libavif command-line distributions.

## Installation

```
dotnet add package Soenneker.Libavif.Util
```

```csharp
await libavif.Encode("input.png", "output.avif", new AvifEncodeOptions
{
    Quality = 80,
    Speed = 6,
    Progressive = true
});
```

The package selects the bundled Windows x64 or Linux x64 `avifenc` runtime automatically. Encoding is written to a temporary file and atomically committed to the requested output path.
