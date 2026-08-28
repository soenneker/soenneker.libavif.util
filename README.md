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

Register the utility through dependency injection:

```csharp
services.AddLibavifUtilAsSingleton();
```

Use `AvifCommand` when direct access to additional `avifenc` options is needed:

```csharp
var command = new AvifCommand();
command.AddFlag("progressive")
       .AddOption("speed", 6)
       .AddArgument("input.png")
       .AddArgument("output.avif");

await libavif.Execute(command);
```

The package uses `RuntimeUtil` to select the bundled Windows x64 or Linux x64 runtime. Filesystem and process operations use the standard Soenneker utilities, command lines are built with `PooledStringBuilder`, and output is written to a unique temporary file before being atomically committed.
