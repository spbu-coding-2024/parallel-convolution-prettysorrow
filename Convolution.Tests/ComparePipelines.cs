namespace Convolution.Tests;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

[Trait("Suite", "Abstract")]
public abstract class Pipeline : IDisposable
{
    private readonly string testDir;

    private readonly ImageGenerator imageGenerator = new();

    private readonly FilterGenerator filterGenerator = new();

    public static readonly Func<string, string> MakeOutputPath1 = path => Path.ChangeExtension(path, ".conv1.png");
    public static readonly Func<string, string> MakeOutputPath2 = path => Path.ChangeExtension(path, ".conv2.png");

    public Pipeline()
    {
        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(this.testDir))
        {
            Directory.Delete(this.testDir, recursive: true);
        }
    }

    private static void CompareResults(List<string> paths)
    {
        foreach (var path in paths)
        {
            using var syncResult = Image.Load<RgbaVector>(MakeOutputPath2(path));
            using var asyncResult = Image.Load<RgbaVector>(MakeOutputPath1(path));
            Assert.True(syncResult.IsEqualTo(asyncResult));
        }
    }

    [Theory]
    [MemberData("ImageParams")]
    public async Task ParallelSync_Matches_SequentialSync((int count, int width, int height) prms)
    {
        var paths = this.imageGenerator.WriteRandomImages(this.testDir, prms.count, prms.width, prms.height);
        var filter = this.filterGenerator.Next();
        Convolution.Impl.Pipeline.ProcessSequentialSync(paths, MakeOutputPath1, filter);
        Convolution.Impl.Pipeline.ProcessParallelSync(paths, MakeOutputPath2, filter);
        CompareResults(paths);
    }

    [Theory]
    [MemberData("ImageParams")]
    public async Task ParallelSync_Matches_ParallelAsync((int count, int width, int height) prms)
    {
        var paths = this.imageGenerator.WriteRandomImages(this.testDir, prms.count, prms.width, prms.height);
        var filter = this.filterGenerator.Next();
        Convolution.Impl.Pipeline.ProcessParallelSync(paths, MakeOutputPath2, filter);
        await Convolution.Impl.Pipeline.ProcessParallelAsync(paths, MakeOutputPath1, filter);
        CompareResults(paths);
    }

    [Theory]
    [MemberData("ImageParams")]
    public async Task ParallelAsync_Matches_SequentialAsync((int count, int width, int height) prms)
    {
        var paths = this.imageGenerator.WriteRandomImages(this.testDir, prms.count, prms.width, prms.height);
        var filter = this.filterGenerator.Next();
        await Convolution.Impl.Pipeline.ProcessSequentialAsync(paths, MakeOutputPath1, filter);
        await Convolution.Impl.Pipeline.ProcessParallelAsync(paths, MakeOutputPath2, filter);
        CompareResults(paths);
    }
}

[Trait("Suite", "All")]
public sealed class Pipeline_All : Pipeline
{
    public static TheoryData<(int count, int width, int height)> ImageParams => new()
        {
            (count: 1, width: 200, height: 100),
            (count: 10, width: 200, height: 100),
        };
}

[Trait("Suite", "Coverage")]
public sealed class Pipeline_Coverage : Pipeline
{
    public static TheoryData<(int count, int width, int height)> ImageParams => new()
        { (count: 3, width: 50, height: 50) };
}
