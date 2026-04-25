namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

/// <summary>
/// Checks that different pixel traversal orders in parallel convolution implementations produce identical results.
/// </summary>
[Trait("Suite", "Abstract")]
public abstract class CompareParallelOrdersTests
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    private static void AssertAllOrdersEqual(Image<RgbaVector> image, Filter filter)
    {
        using var rowsResult = Impl.Parallel.ApplyRows(image, filter);
        using var columnsResult = Impl.Parallel.ApplyColumns(image, filter);
        using var tilesResult = Impl.Parallel.ApplyTiles(image, filter);

        Assert.True(rowsResult.IsEqualTo(columnsResult));
        Assert.True(columnsResult.IsEqualTo(tilesResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_IdentityFilter(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        AssertAllOrdersEqual(image, Filters.Identity);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_BoxBlur(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        AssertAllOrdersEqual(image, Filters.BoxBlur(radius: 1));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_GaussianBlur(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        AssertAllOrdersEqual(image, Filters.GaussianBlur(sigma: 0.5f));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_SobelX(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        AssertAllOrdersEqual(image, Filters.SobelX);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_SobelY(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        AssertAllOrdersEqual(image, Filters.SobelY);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_PrewittX(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        AssertAllOrdersEqual(image, Filters.PrewittX);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_PrewittY(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        AssertAllOrdersEqual(image, Filters.PrewittY);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_RandomFilter(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter = FilterGenerator.Next(size: 5);
        AssertAllOrdersEqual(image, filter);
    }
}

[Trait("Suite", "Coverage")]
public class CompareParallelOrdersTests_Coverage : CompareParallelOrdersTests
{
    public static TheoryData<int, int> ImageSizes => new() { { 50, 50 } };
}

[Trait("Suite", "All")]
public class CompareParallelOrdersTests_All : CompareParallelOrdersTests
{
    public static TheoryData<int, int> ImageSizes => new()
    {
        { 1, 1 },
        { 32, 32 },
        { 256, 144 },
        { 640, 360 },
    };
}