namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

/// <summary>
/// Checks that different pixel traversal orders in parallel convolution implementations produce identical results.
/// </summary>
public class CompareParallelOrders
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    private static void RunSingle(Image<RgbaVector> image, Filter filter)
    {
        using var rowsResult = Impl.Parallel.ApplyRows(image, filter);
        using var columnsResult = Impl.Parallel.ApplyColumns(image, filter);
        using var tilesResult = Impl.Parallel.ApplyTiles(image, filter);

        Assert.True(rowsResult.IsEqualTo(columnsResult));
        Assert.True(columnsResult.IsEqualTo(tilesResult));
    }

    [Fact]
    public void Apply_IdentityFilter()
    {
        using var image = ImageGenerator.Next();
        var filter = Filters.Identity;
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_BoxBlur()
    {
        using var image = ImageGenerator.Next();
        var filter = Filters.BoxBlur(radius: 1);
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_GaussianBlur()
    {
        using var image = ImageGenerator.Next();
        var filter = Filters.GaussianBlur(sigma: 0.5f);
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_SobelX()
    {
        using var image = ImageGenerator.Next();
        var filter = Filters.SobelX;
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_SobelY()
    {
        using var image = ImageGenerator.Next();
        var filter = Filters.SobelY;
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_PrewittX()
    {
        using var image = ImageGenerator.Next();
        var filter = Filters.PrewittX;
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_PrewittY()
    {
        using var image = ImageGenerator.Next();
        var filter = Filters.PrewittY;
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_RandomFilter5()
    {
        using var image = ImageGenerator.Next();
        var filter = FilterGenerator.Next(size: 5);
        RunSingle(image, filter);
    }

    [Fact]
    public void Apply_RandomFilter25()
    {
        using var image = ImageGenerator.Next();
        var filter = FilterGenerator.Next(size: 25);
        RunSingle(image, filter);
    }
}
