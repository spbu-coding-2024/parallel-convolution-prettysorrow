namespace Convolution.Tests;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

/// <summary>
/// Checks that different pixel traversal orders in parallel convolution implementations produce identical results.
/// </summary>
[Trait("Suite", "Abstract")]
public abstract class CompareUnsafeParallel
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    private static void RunSingle(Image<RgbaVector> image, Filter filter)
    {
        using var unsafeResult = Convolution.Impl.Unsafe.Apply(filter, image);
        using var parallelResult = Convolution.Impl.Parallel.Apply(filter, image);

        Assert.True(unsafeResult.IsEqualTo(parallelResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_IdentityFilter((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = Filters.Identity;
        RunSingle(image, filter);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_BoxBlur((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = Filters.BoxBlur(radius: 1);
        RunSingle(image, filter);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_GaussianBlur((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = Filters.GaussianBlur(sigma: 0.5f);
        RunSingle(image, filter);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_SobelX((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = Filters.SobelX;
        RunSingle(image, filter);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_SobelY((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = Filters.SobelY;
        RunSingle(image, filter);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_PrewittX((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = Filters.PrewittX;
        RunSingle(image, filter);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_PrewittY((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = Filters.PrewittY;
        RunSingle(image, filter);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_RandomFilter((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = FilterGenerator.Next(size: 5);
        RunSingle(image, filter);
    }
}

[Trait("Suite", "Coverage")]
public class CompareUnsafeParallel_Coverage : CompareUnsafeParallel
{
    public static TheoryData<(int width, int height)> ImageSizes => new() { (width: 50, height: 50) };
}

[Trait("Suite", "All")]
public class CompareUnsafeParallel_All : CompareUnsafeParallel
{
    public static TheoryData<(int width, int height)> ImageSizes => new()
    {
        (width: 1, height: 1),
        (width: 32, height: 32),
        (width: 256, height: 144),
        (width: 640, height: 360),
    };
}
