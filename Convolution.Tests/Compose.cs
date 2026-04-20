namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using Xunit;

/// <summary>
/// Tests for Convolution.Core.Filter.Compose.
/// </summary>
public class Compose
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    [Fact]
    public void Compose_Identity()
    {
        using var image = ImageGenerator.Next();

        var composition = Filter.Compose(Filters.Identity, Filters.Identity);
        using var output = Impl.Sequential.Apply(image, composition);

        Assert.True(output.IsEqualTo(image));
    }

    [Fact]
    public void Compose_Commutativity_Wrap()
    {
        using var image = ImageGenerator.Next();
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);

        var composition1 = Filter.Compose(filter1, filter2);
        var composition2 = Filter.Compose(filter2, filter1);

        using var output1 = Impl.Sequential.Apply(image, composition1);
        using var output2 = Impl.Sequential.Apply(image, composition2);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Fact]
    public void Compose_Commutativity_Clamp()
    {
        using var image = ImageGenerator.Next();
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Clamp);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Clamp);

        var composition1 = Filter.Compose(filter1, filter2);
        var composition2 = Filter.Compose(filter2, filter1);

        using var output1 = Impl.Sequential.Apply(image, composition1);
        using var output2 = Impl.Sequential.Apply(image, composition2);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Fact]
    public void Compose_Compositionality_Wrap()
    {
        using var image = ImageGenerator.Next();
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);

        var composition = Filter.Compose(filter1, filter2);

        using var output1 = Impl.Sequential.Apply(Impl.Sequential.Apply(image, filter1), filter2);
        using var output2 = Impl.Sequential.Apply(image, composition);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Fact]
    public void Compose_Shift_LeftRight()
    {
        using var image = ImageGenerator.Next();

        using var leftRight = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftLeft, Filters.ShiftRight));
        using var rightLeft = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftRight, Filters.ShiftLeft));

        Assert.True(leftRight.IsEqualTo(image));
        Assert.True(rightLeft.IsEqualTo(image));
    }

    [Fact]
    public void Compose_Shift_TopBottop()
    {
        using var image = ImageGenerator.Next();

        using var topBottom = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftTop, Filters.ShiftBottom));
        using var bottomTop = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftBottom, Filters.ShiftTop));

        Assert.True(topBottom.IsEqualTo(image));
        Assert.True(bottomTop.IsEqualTo(image));
    }

    [Fact]
    public void Compose_Shift_LeftLeftRight()
    {
        using var image = ImageGenerator.Next();

        using var llr = Impl.Sequential.Apply(image, Filter.Compose(Filter.Compose(Filters.ShiftLeft, Filters.ShiftLeft), Filters.ShiftRight));
        using var lrl = Impl.Sequential.Apply(image, Filter.Compose(Filter.Compose(Filters.ShiftLeft, Filters.ShiftRight), Filters.ShiftLeft));
        using var rll = Impl.Sequential.Apply(image, Filter.Compose(Filter.Compose(Filters.ShiftRight, Filters.ShiftLeft), Filters.ShiftLeft));

        Assert.True(llr.IsEqualTo(lrl));
        Assert.True(lrl.IsEqualTo(rll));
    }

    [Fact]
    public void Compose_TopRight()
    {
        using var image = ImageGenerator.Next();

        using var applyComposeTopRight = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftTop(10), Filters.ShiftRight(10)));
        using var applyComposeRightTop = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftRight(10), Filters.ShiftTop(10)));
        using var applyApplyTopRight = Impl.Sequential.Apply(Impl.Sequential.Apply(image, Filters.ShiftTop(10)), Filters.ShiftRight(10));
        using var applyApplyRightTop = Impl.Sequential.Apply(Impl.Sequential.Apply(image, Filters.ShiftRight(10)), Filters.ShiftTop(10));

        Assert.True(applyComposeTopRight.IsEqualTo(applyComposeRightTop));
        Assert.True(applyComposeRightTop.IsEqualTo(applyApplyTopRight));
        Assert.True(applyApplyTopRight.IsEqualTo(applyApplyRightTop));
    }
}
