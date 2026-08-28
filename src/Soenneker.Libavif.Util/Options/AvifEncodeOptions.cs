using System;

namespace Soenneker.Libavif.Util.Options;

/// <summary>Controls AVIF encoding.</summary>
public sealed class AvifEncodeOptions
{
    /// <summary>Color quality from 0 (worst) through 100 (lossless).</summary>
    public int Quality { get; init; } = 80;

    /// <summary>Optional alpha quality from 0 through 100. Defaults to <see cref="Quality"/>.</summary>
    public int? AlphaQuality { get; init; }

    /// <summary>Encoder speed from 0 (slowest) through 10 (fastest).</summary>
    public int Speed { get; init; } = 6;

    /// <summary>Enables lossless encoding.</summary>
    public bool Lossless { get; init; }

    /// <summary>Creates a layered AVIF that supports progressive rendering.</summary>
    public bool Progressive { get; init; }

    /// <summary>Removes EXIF, XMP, and ICC metadata from the output.</summary>
    public bool StripMetadata { get; init; } = true;

    /// <summary>Validates the encoder settings.</summary>
    public void Validate()
    {
        if (Quality is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Quality), "Quality must be between 0 and 100.");

        if (AlphaQuality is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(AlphaQuality), "Alpha quality must be between 0 and 100.");

        if (Speed is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(Speed), "Speed must be between 0 and 10.");
    }
}
