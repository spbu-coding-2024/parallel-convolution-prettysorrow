namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp.Processing;
using Xunit;

/// <summary>
/// Compares sequential implementation and SixLabors' one.
/// </summary>
[Trait("Suite", "Abstract")]
public abstract class CompareImageSharpSequentialTests
{
    private static readonly ImageGenerator ImageGenerator = new();

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_BoxBlur(int width, int height)
    {
        int radius = 1;
        using var input = ImageGenerator.Next(width, height);
        using var seqResult = Impl.Sequential.Apply(input, Filters.BoxBlur(radius));
        using var sharpResult = input.Clone(ctx => ctx.BoxBlur(radius));
        Assert.True(seqResult.IsEqualTo(sharpResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_GaussianBlur(int width, int height)
    {
        float sigma = 1.5f;
        using var input = ImageGenerator.Next(width, height);
        using var seqResult = Impl.Sequential.Apply(input, Filters.GaussianBlur(sigma));
        using var sharpResult = input.Clone(ctx => ctx.GaussianBlur(sigma));
        Assert.True(seqResult.IsEqualTo(sharpResult));
    }
}

[Trait("Suite", "Coverage")]
public class CompareImageSharpSequentialTests_Coverage : CompareImageSharpSequentialTests
{
    public static TheoryData<int, int> ImageSizes => new() { { 50, 50 } };
}

[Trait("Suite", "All")]
public class CompareImageSharpSequentialTests_All : CompareImageSharpSequentialTests
{
    public static TheoryData<int, int> ImageSizes => new()
    {
        { 32, 32 },
        { 256, 144 },
        { 640, 360 },
    };
}
