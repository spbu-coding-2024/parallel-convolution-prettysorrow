namespace Convolution.Extensions;

using System;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Provides extension methods for SixLabors.ImageSharp.Image.
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Compares two SixLabors.ImageSharp.Image(TPixel).
    /// </summary>
    public static bool IsEqualTo<TPixel>(this Image<TPixel> source, Image<TPixel> other, Func<TPixel, TPixel, bool> pixelComparer)
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
                if (!pixelComparer(source[x, y], other[x, y]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Compares two SixLabors.ImageSharp.Image(RgbaVector).
    /// </summary>
    public static bool IsEqualTo(this Image<RgbaVector> source, Image<RgbaVector> other, float tolerance = 1e-5f)
    {
        bool Comparer(RgbaVector p1, RgbaVector p2) =>
            MathF.Abs(p1.R - p2.R) <= tolerance &&
            MathF.Abs(p1.G - p2.G) <= tolerance &&
            MathF.Abs(p1.B - p2.B) <= tolerance &&
            MathF.Abs(p1.A - p2.A) <= tolerance;

        return source.IsEqualTo(other, Comparer);
    }

    /// <summary>
    /// Evaluates a single pixel using convolution.
    /// </summary>
    public static RgbaVector FilterOnePixel(this Image<RgbaVector> source, Filter filter, int x, int y)
    {
        static int Wrap(int n, int bound) => ((n % bound) + bound) % bound;

        int size = filter.Kernel.GetLength(0);
        int offset = size / 2;
        float r = 0, g = 0, b = 0;

        for (int ky = 0; ky < size; ky++)
        {
            for (int kx = 0; kx < size; kx++)
            {
                int srcX = x + kx - offset;
                int srcY = y + ky - offset;

                switch (filter.EdgeMode)
                {
                    case EdgeMode.Clamp:
                        srcX = Math.Clamp(srcX, 0, source.Width - 1);
                        srcY = Math.Clamp(srcY, 0, source.Height - 1);
                        break;
                    case EdgeMode.Wrap:
                        srcX = Wrap(srcX, source.Width);
                        srcY = Wrap(srcY, source.Height);
                        break;
                }

                var pixel = source[srcX, srcY];
                float weight = filter.Kernel[ky, kx];
                r += pixel.R * weight;
                g += pixel.G * weight;
                b += pixel.B * weight;
            }
        }

        float factor = (float)filter.Factor;
        float bias = (float)filter.Bias;
        return new RgbaVector((factor * r) + bias, (factor * g) + bias, (factor * b) + bias);
    }
}
