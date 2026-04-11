namespace Convolution.Tests;

using System.Drawing;
using Convolution.Core;
using Convolution.Extensions;
using Xunit;

public class Compose
{
    private static readonly ImageGenerator imageGenerator = new();
    private static readonly FilterGenerator filterGenerator = new();

    [Fact]
    public void Compose_Commutativity()
    {
        var filter1 = filterGenerator.Next(size: 3);
        var filter2 = filterGenerator.Next(size: 3);

        var composition1 = Filter.Compose(filter1, filter2);
        var composition2 = Filter.Compose(filter2, filter1);

        using var image = imageGenerator.Next();

        using var output1 = Impl.Sequential.Apply(image, composition1);
        using var output2 = Impl.Sequential.Apply(image, composition2);

        Assert.True(output1.Equal(output2));
    }
}