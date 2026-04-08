namespace Convolution.Impl;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class Sequential
{
    public static Image<Rgb24> Apply(Image<Rgb24> source, Filter filter)
    {
        var result = new Image<Rgb24>(source.Width, source.Height);

        // TODO? provide ability to change pixels processing order
        for (int y = 0; y < source.Height; y++)
            for (int x = 0; x < source.Width; x++)
                result[x, y] = filter.ApplyToRgb24(source, x, y);


        return result;
    }
}