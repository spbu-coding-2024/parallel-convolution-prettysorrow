namespace Convolution.Impl;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class Sequential
{
    public static Image<RgbaVector> Apply(Image<RgbaVector> source, Filter filter)
    {
        var result = new Image<RgbaVector>(source.Width, source.Height);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                result[x, y] = source.FilterOnePixel(filter, x, y);
            }
        }

        return result;
    }
}
