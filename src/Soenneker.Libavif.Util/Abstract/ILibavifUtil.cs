using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Libavif.Util.Options;

namespace Soenneker.Libavif.Util.Abstract;

/// <summary>A structured, cross-platform API for the bundled libavif command-line tools.</summary>
public interface ILibavifUtil
{
    /// <summary>Runs <c>avifenc</c> with raw command-line arguments.</summary>
    ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the bundled <c>avifenc</c> version.</summary>
    ValueTask<string> GetVersion(CancellationToken cancellationToken = default);

    /// <summary>Encodes a JPEG, PNG, or Y4M input as AVIF.</summary>
    ValueTask Encode(string inputPath, string outputPath, AvifEncodeOptions? options = null,
        CancellationToken cancellationToken = default);
}
