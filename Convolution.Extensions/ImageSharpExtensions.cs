namespace Convolution.Extensions;

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class ImageSharpExtensions
{
    public static bool Equal<TPixel>(this Image<TPixel> source, Image<TPixel> other, Func<TPixel, TPixel, bool> comparer)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (ReferenceEquals(source, other))
        {
            return true;
        }

        if (source.Width != other.Width || source.Height != other.Height)
        {
            return false;
        }

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                if (!comparer(source[x, y], other[x, y]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool Equal(this Image<RgbaVector> source, Image<RgbaVector> other, float tolerance = 1e-5f) // TODO: is tolerance OK ??
    {
        bool Comparer(RgbaVector p1, RgbaVector p2) =>
            MathF.Abs(p1.R - p2.R) <= tolerance &&
            MathF.Abs(p1.G - p2.G) <= tolerance &&
            MathF.Abs(p1.B - p2.B) <= tolerance &&
            MathF.Abs(p1.A - p2.A) <= tolerance;

        return source.Equal(other, Comparer);
    }
}
