namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using Xunit;

[Trait("Suite", "Abstract")]
public abstract class ComposeTests
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Identity(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter = Filter.Compose(Filters.Identity, Filters.Identity);
        using var output = Impl.Parallel.Apply(image, filter);
        Assert.True(output.IsEqualTo(image));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Commutativity_Wrap(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);

        var composition1 = Filter.Compose(filter1, filter2);
        var composition2 = Filter.Compose(filter2, filter1);

        using var output1 = Impl.Parallel.Apply(image, composition1);
        using var output2 = Impl.Parallel.Apply(image, composition2);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Commutativity_Clamp(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Clamp);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Clamp);

        var composition1 = Filter.Compose(filter1, filter2);
        var composition2 = Filter.Compose(filter2, filter1);

        using var output1 = Impl.Parallel.Apply(image, composition1);
        using var output2 = Impl.Parallel.Apply(image, composition2);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Compositionality_Wrap(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);

        var composition = Filter.Compose(filter1, filter2);
        using var output1 = Impl.Parallel.Apply(Impl.Parallel.Apply(image, filter1), filter2);
        using var output2 = Impl.Parallel.Apply(image, composition);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Shift_LeftRight(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);

        using var leftRightResult = Impl.Parallel.Apply(image, Filter.Compose(Filters.ShiftLeft(5), Filters.ShiftRight(5)));
        using var rightLeftResult = Impl.Parallel.Apply(image, Filter.Compose(Filters.ShiftRight(5), Filters.ShiftLeft(5)));

        Assert.True(leftRightResult.IsEqualTo(image));
        Assert.True(rightLeftResult.IsEqualTo(image));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Shift_TopBottom(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        using var topBottom = Impl.Parallel.Apply(image, Filter.Compose(Filters.ShiftTop(5), Filters.ShiftBottom(5)));
        using var bottomTop = Impl.Parallel.Apply(image, Filter.Compose(Filters.ShiftBottom(5), Filters.ShiftTop(5)));
        Assert.True(topBottom.IsEqualTo(image));
        Assert.True(bottomTop.IsEqualTo(image));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Shift_LeftLeftRight(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var llrFilter = Filter.Compose(Filter.Compose(Filters.ShiftLeft(5), Filters.ShiftLeft(5)), Filters.ShiftRight(5));
        var lrlFilter = Filter.Compose(Filter.Compose(Filters.ShiftLeft(5), Filters.ShiftRight(5)), Filters.ShiftLeft(5));
        var rllFilter = Filter.Compose(Filter.Compose(Filters.ShiftRight(5), Filters.ShiftLeft(5)), Filters.ShiftLeft(5));

        using var llrResult = Impl.Parallel.Apply(image, llrFilter);
        using var lrlResult = Impl.Parallel.Apply(image, lrlFilter);
        using var rllResult = Impl.Parallel.Apply(image, rllFilter);

        Assert.True(llrResult.IsEqualTo(lrlResult));
        Assert.True(lrlResult.IsEqualTo(rllResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_TopRight(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);

        using var applyComposeTopRight = Impl.Parallel.Apply(image, Filter.Compose(Filters.ShiftTop(10), Filters.ShiftRight(10)));
        using var applyComposeRightTop = Impl.Parallel.Apply(image, Filter.Compose(Filters.ShiftRight(10), Filters.ShiftTop(10)));
        using var applyApplyTopRight = Impl.Parallel.Apply(Impl.Parallel.Apply(image, Filters.ShiftTop(10)), Filters.ShiftRight(10));
        using var applyApplyRightTop = Impl.Parallel.Apply(Impl.Parallel.Apply(image, Filters.ShiftRight(10)), Filters.ShiftTop(10));

        Assert.True(applyComposeTopRight.IsEqualTo(applyComposeRightTop));
        Assert.True(applyComposeRightTop.IsEqualTo(applyApplyTopRight));
        Assert.True(applyApplyTopRight.IsEqualTo(applyApplyRightTop));
    }
}

[Trait("Suite", "Coverage")]
public class ComposeTests_Coverage : ComposeTests
{
    public static TheoryData<int, int> ImageSizes => new() { { 50, 50 } };
}

[Trait("Suite", "All")]
public class ComposeTests_All : ComposeTests
{
    public static TheoryData<int, int> ImageSizes => new()
    {
        { 1, 1 },
        { 32, 32 },
        { 256, 144 },
        { 640, 360 },
    };
}