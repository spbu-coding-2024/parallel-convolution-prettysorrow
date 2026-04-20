namespace Convolution.Measurement;

using BenchmarkDotNet.Attributes;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

[MemoryDiagnoser]
[MarkdownExporter]
[CsvExporter]
[SimpleJob(warmupCount: 1, iterationCount: 5)]
[Config(typeof(BenchmarkConfig))]
public class RandomBenchmark
{
    [Params(128, 256)]
    public int ImageSize { get; set; }

    [Params(5, 13)]
    public int FilterSize { get; set; }

    private readonly ImageGenerator imageGenerator = new(seed: 42);
    private readonly FilterGenerator filterGenerator = new(seed: 42);
    private Image<RgbaVector> sourceImage = null!;
    private Filter filter = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.sourceImage = this.imageGenerator.Next(width: this.ImageSize, height: this.ImageSize);
        this.filter = this.filterGenerator.Next(size: this.FilterSize);
    }

    [GlobalCleanup]
    public void Cleanup()
        => this.sourceImage.Dispose();

    [Benchmark(Baseline = true)]
    public Image<RgbaVector> Sequential()
        => Impl.Sequential.Apply(this.sourceImage, this.filter);

    [Benchmark]
    public Image<RgbaVector> Parallel_Rows()
        => Impl.Parallel.ApplyRows(this.sourceImage, this.filter);

    [Benchmark]
    public Image<RgbaVector> Parallel_Columns()
        => Impl.Parallel.ApplyColumns(this.sourceImage, this.filter);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size8()
        => Impl.Parallel.ApplyTiles(this.sourceImage, this.filter, tileSize: 8);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size128()
   => Impl.Parallel.ApplyTiles(this.sourceImage, this.filter, tileSize: 128);
}
