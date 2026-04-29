namespace Convolution.Tests;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

/// <summary>
/// Compares Convolution.Impl.Parallel.Apply and Convolution.Impl.Sequential.Apply.
/// </summary>
[Trait("Suite", "Abstract")]
public abstract class CompareSequentialParallel
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    private static void RunSingle(Image<RgbaVector> image, Filter filter)
    {
        using var parallelResult = Impl.Parallel.Apply(filter, image);
        using var sequentialResult = Impl.Sequential.Apply(filter, image);
        Assert.True(parallelResult.IsEqualTo(sequentialResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_IdentityFilter(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        RunSingle(image, Filters.Identity);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_BoxBlur(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        RunSingle(image, Filters.BoxBlur(radius: 1));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_GaussianBlur(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        RunSingle(image, Filters.GaussianBlur(sigma: 0.5f));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_SobelX(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        RunSingle(image, Filters.SobelX);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_SobelY(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        RunSingle(image, Filters.SobelY);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_PrewittX(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        RunSingle(image, Filters.PrewittX);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_PrewittY(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        RunSingle(image, Filters.PrewittY);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_RandomFilter(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter = FilterGenerator.Next(size: 3);
        RunSingle(image, filter);
    }
}

[Trait("Suite", "Coverage")]
public class CompareSequentialParallel_Coverage : CompareSequentialParallel
{
    public static TheoryData<int, int> ImageSizes => new() { { 50, 50 } };
}

[Trait("Suite", "All")]
public class CompareSequentialParallel_All : CompareSequentialParallel
{
    public static TheoryData<int, int> ImageSizes => new()
    {
        { 1, 1 },
        { 32, 32 },
        { 256, 144 },
        { 640, 360 },
    };
}
