namespace Convolution.Tests;

using Convolution.Core;
using Xunit;

/// <summary>
/// Checks compliance with simple invariants for several image convolution implementations.
/// </summary>
[Trait("Suite", "Abstract")]
public abstract class InvariantTests
{
    private static readonly ImageGenerator ImageGenerator = new(seed: 42);
    private static readonly FilterGenerator FilterGenerator = new(seed: 42);

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_Zeros_OnDifferentImages_ReturnsSameImage((int width, int height) imageSize)
    {
        using var image1 = ImageGenerator.Next(imageSize.width, imageSize.height);
        using var image2 = ImageGenerator.Next(imageSize.width, imageSize.height);

        using var seqResultImage1 = Convolution.Impl.Sequential.Apply(Filters.Zeros, image1);
        using var seqResultImage2 = Convolution.Impl.Sequential.Apply(Filters.Zeros, image2);
        using var parResultImage1 = Convolution.Impl.Parallel.Apply(Filters.Zeros, image1);
        using var parResultImage2 = Convolution.Impl.Parallel.Apply(Filters.Zeros, image2);
        using var unsafeResultImage1 = Convolution.Impl.Unsafe.Apply(Filters.Zeros, image1);
        using var unsafeResultImage2 = Convolution.Impl.Unsafe.Apply(Filters.Zeros, image2);

        Assert.True(seqResultImage1.IsEqualTo(seqResultImage2));
        Assert.True(parResultImage1.IsEqualTo(parResultImage2));
        Assert.True(unsafeResultImage1.IsEqualTo(unsafeResultImage2));

        Assert.True(seqResultImage1.IsEqualTo(parResultImage1));
        Assert.True(seqResultImage1.IsEqualTo(unsafeResultImage1));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_IdentityFilter_ReturnsSameImage((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);

        using var seqResult = Convolution.Impl.Sequential.Apply(Filters.Identity, image);
        using var parResult = Convolution.Impl.Parallel.Apply(Filters.Identity, image);
        using var unsafeResult = Convolution.Impl.Unsafe.Apply(Filters.Identity, image);

        Assert.True(image.IsEqualTo(seqResult));
        Assert.True(image.IsEqualTo(parResult));
        Assert.True(image.IsEqualTo(unsafeResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_RandomFilter_ReturnsDifferentImage((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = FilterGenerator.Next();

        using var seqResult = Convolution.Impl.Sequential.Apply(filter, image);
        using var parResult = Convolution.Impl.Parallel.Apply(filter, image);
        using var unsafeResult = Convolution.Impl.Unsafe.Apply(filter, image);

        Assert.False(image.IsEqualTo(seqResult));
        Assert.False(image.IsEqualTo(parResult));
        Assert.False(image.IsEqualTo(unsafeResult));

        Assert.True(seqResult.IsEqualTo(parResult));
        Assert.True(seqResult.IsEqualTo(unsafeResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_RandomFilter_DoesNotChangeImageSize((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var filter = FilterGenerator.Next();

        using var seqResult = Convolution.Impl.Sequential.Apply(filter, image);
        using var parResult = Convolution.Impl.Parallel.Apply(filter, image);
        using var unsafeResult = Convolution.Impl.Unsafe.Apply(filter, image);

        Assert.Equal(image.Width, seqResult.Width);
        Assert.Equal(image.Height, seqResult.Height);

        Assert.Equal(image.Width, parResult.Width);
        Assert.Equal(image.Height, parResult.Height);

        Assert.Equal(image.Width, unsafeResult.Width);
        Assert.Equal(image.Height, unsafeResult.Height);
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Pad_DoesNotChangeResult((int width, int height) imageSize)
    {
        using var image = ImageGenerator.Next(imageSize.width, imageSize.height);
        var defaultFilter = FilterGenerator.Next();
        var paddedFilter1 = defaultFilter.Pad(padding: 1);
        var paddedFilter5 = defaultFilter.Pad(padding: 5);

        using var seqResultDefault = Convolution.Impl.Sequential.Apply(defaultFilter, image);
        using var seqResultPad1 = Convolution.Impl.Sequential.Apply(paddedFilter1, image);
        using var seqResultPad5 = Convolution.Impl.Sequential.Apply(paddedFilter5, image);

        using var parResultDefault = Convolution.Impl.Parallel.Apply(defaultFilter, image);
        using var parResultPad1 = Convolution.Impl.Parallel.Apply(paddedFilter1, image);
        using var parResultPad5 = Convolution.Impl.Parallel.Apply(paddedFilter5, image);

        using var unsafeResultDefault = Convolution.Impl.Unsafe.Apply(defaultFilter, image);
        using var unsafeResultPad1 = Convolution.Impl.Unsafe.Apply(paddedFilter1, image);
        using var unsafeResultPad5 = Convolution.Impl.Unsafe.Apply(paddedFilter5, image);

        Assert.True(seqResultDefault.IsEqualTo(seqResultPad1));
        Assert.True(seqResultPad1.IsEqualTo(seqResultPad5));

        Assert.True(parResultDefault.IsEqualTo(parResultPad1));
        Assert.True(parResultPad1.IsEqualTo(parResultPad5));

        Assert.True(unsafeResultDefault.IsEqualTo(unsafeResultPad1));
        Assert.True(unsafeResultPad1.IsEqualTo(unsafeResultPad5));

        Assert.True(seqResultDefault.IsEqualTo(parResultDefault));
        Assert.True(seqResultDefault.IsEqualTo(unsafeResultDefault));
    }
}

[Trait("Suite", "Coverage")]
public class InvariantTests_Coverage : InvariantTests
{
    public static TheoryData<(int width, int height)> ImageSizes => new() { (width: 50, height: 50) };
}

[Trait("Suite", "All")]
public class InvariantTests_All : InvariantTests
{
    public static TheoryData<(int width, int height)> ImageSizes => new()
    {
        (width: 1, height: 1),
        (width: 32, height: 32),
        (width: 256, height: 144),
        (width: 640, height: 360),
    };
}
