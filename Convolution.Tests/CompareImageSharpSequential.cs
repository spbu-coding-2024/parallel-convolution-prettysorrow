namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp.Processing;
using Xunit;

/// <summary>
/// Tests which compare sequential implementation and SixLabors' one.
/// </summary>
public class CompareImageSharpSequential
{
    private static readonly ImageGenerator ImageGenerator = new();

    [Fact]
    public void Apply_BoxBlur()
    {
        int radius = 1;

        using var input = ImageGenerator.Next();

        using var seqResult = Impl.Sequential.Apply(input, Filters.BoxBlur(radius));
        using var sharpResult = input.Clone(ctx => ctx.BoxBlur(radius));

        Assert.True(seqResult.IsEqualTo(sharpResult));
    }

    [Fact]
    public void Apply_GaussianBlur()
    {
        float sigma = 1.5f;

        using var input = ImageGenerator.Next();

        using var seqResult = Impl.Sequential.Apply(input, Filters.GaussianBlur(sigma));
        using var sharpResult = input.Clone(ctx => ctx.GaussianBlur(sigma));

        Assert.True(seqResult.IsEqualTo(sharpResult));
    }
}
