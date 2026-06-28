namespace Convolution.Measurement;

using BenchmarkDotNet.Attributes;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Runs on random image and random filter and compares different convolution implementations.
/// </summary>
[MarkdownExporter]
[CsvExporter]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
[Config(typeof(BenchmarkConfig))]
public class Benchmark
{
    [Params(1)]
    public int ImageCount { get; set; }

    [Params(16, 32)]
    public int ImageSize { get; set; }

    [Params(5, 13, 21)]
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
        => Convolution.Impl.Sequential.Apply(this.filter, this.sourceImage);

    [Benchmark]
    public Image<RgbaVector> Parallel_Rows()
        => Convolution.Impl.Parallel.ApplyRows(this.filter, this.sourceImage);

    [Benchmark]
    public Image<RgbaVector> Parallel_Columns()
        => Convolution.Impl.Parallel.ApplyColumns(this.filter, this.sourceImage);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size8()
        => Convolution.Impl.Parallel.ApplyTiles(this.filter, this.sourceImage, tileSize: 8);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size32()
        => Convolution.Impl.Parallel.ApplyTiles(this.filter, this.sourceImage, tileSize: 32);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size128()
        => Convolution.Impl.Parallel.ApplyTiles(this.filter, this.sourceImage, tileSize: 128);

    [Benchmark]
    public Image<RgbaVector> Unsafe()
        => Convolution.Impl.Unsafe.Apply(this.filter, this.sourceImage);
}
