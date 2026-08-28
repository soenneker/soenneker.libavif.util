using System;
using System.IO;
using System.Threading.Tasks;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Libavif.Util.Options;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Libavif.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class LibavifUtilTests : HostedUnitTest
{
    private readonly ILibavifUtil _util;

    public LibavifUtilTests(Host host) : base(host)
    {
        _util = Resolve<ILibavifUtil>(true);
    }

    [Test]
    public async Task Resolves()
    {
        await Assert.That(_util).IsNotNull();
    }

    [Test]
    public async Task Rejects_invalid_speed()
    {
        var options = new AvifEncodeOptions {Speed = 11};
        await Assert.That(options.Validate).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Encodes_progressive_avif()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"soenneker-libavif-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string output = Path.Combine(directory, "output.avif");

        try
        {
            await _util.Encode(Path.Combine(AppContext.BaseDirectory, "icon.png"), output,
                new AvifEncodeOptions {Quality = 70, Speed = 10, Progressive = true});
            await Assert.That(File.Exists(output)).IsTrue();
            await Assert.That(new FileInfo(output).Length).IsGreaterThan(0);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
