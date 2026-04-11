namespace Convolution.Measurement;

using BenchmarkDotNet.Attributes;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

[MemoryDiagnoser]
[MarkdownExporter]
[CsvExporter]
[SimpleJob(warmupCount: 1, iterationCount: 5)]
public class RandomBenchmark
{
    [Params(512, 1024)]
    public int ImageSize { get; set; }

    [Params(3, 5, 21)]
    public int FilterSize { get; set; }

    private readonly ImageGenerator _imageGenerator = new(seed: 42);
    private readonly FilterGenerator _filterGenerator = new(seed: 42);
    private Image<Rgb24> _sourceImage = null!;
    private Filter _filter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceImage = _imageGenerator.Next(width: ImageSize, height: ImageSize);
        _filter = _filterGenerator.Next(size: FilterSize);
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
