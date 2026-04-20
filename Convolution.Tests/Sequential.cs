namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using Xunit;

/// <summary>
/// Checks simple invariants that a sequential implementation must satisfy.
/// </summary>
public class Sequential
{
    private static readonly ImageGenerator ImageGenerator = new();

    [Fact]
    public void Apply_IdentityFilter_ReturnsSameImage()
    {
        using var input = ImageGenerator.Next();
        using var result = Impl.Sequential.Apply(input, Filters.Identity);

        Assert.Equal(input.Width, result.Width);
        Assert.Equal(input.Height, result.Height);
        Assert.True(input.Equal(result));
    }

    [Fact]
    public void Apply_BoxBlur_ReturnsDifferentImage()
    {
        using var input = ImageGenerator.Next();
        using var result = Impl.Sequential.Apply(input, Filters.BoxBlur(radius: 1));

        Assert.Equal(input.Width, result.Width);
        Assert.Equal(input.Height, result.Height);

        Assert.False(input.Equal(result));
    }
}