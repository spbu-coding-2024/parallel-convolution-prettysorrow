namespace Convolution.Measurement;

using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// it does not use toplevel statements because it is inside of file-scoped namespace
public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<BlurBenchmark>();
    }

}

[MemoryDiagnoser]
[RPlotExporter]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class BlurBenchmark
{
    [Params(512, 1024)]
    public int ImageSize { get; set; }

    [Params(1, 3, 5)]
    public int BlurRadius { get; set; }

    private readonly ImageGenerator _imageGenerator = new(seed: 42);
    private Image<Rgb24> _sourceImage = null!;
    private Filter _filter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceImage = _imageGenerator.Generate(width: ImageSize, height: ImageSize);
        _filter = Filter.BoxBlur(radius: BlurRadius);
    }


    [GlobalCleanup]
    public void Cleanup()
        => _sourceImage.Dispose();


    [Benchmark(Baseline = true)]
    public Image<Rgb24> Sequential()
        => Impl.Sequential.Apply(_sourceImage, _filter);


    [Benchmark]
    public Image<Rgb24> Parallel_Rows()
        => Impl.Parallel.ApplyRows(_sourceImage, _filter);

    [Benchmark]
    public Image<Rgb24> Parallel_Columns()
        => Impl.Parallel.ApplyColumns(_sourceImage, _filter);

    [Benchmark]
    public Image<Rgb24> Parallel_Tiles()
        => Impl.Parallel.ApplyTiles(_sourceImage, _filter, tileSize: 64);
}
