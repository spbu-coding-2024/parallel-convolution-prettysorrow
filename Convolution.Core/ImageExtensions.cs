namespace Convolution.Core;

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Provides extension methods for SixLabors.ImageSharp.Image.
/// </summary>
public static class ImageExtensions
{
    public static bool IsEqualTo<TPixel>(this Image<TPixel> self, Image<TPixel> other, Func<TPixel, TPixel, bool> pixelComparer)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (ReferenceEquals(self, other))
        {
            return true;
        }

        if (self.Width != other.Width || self.Height != other.Height)
        {
            return false;
        }

        for (int y = 0; y < self.Height; y++)
        {
            for (int x = 0; x < self.Width; x++)
            {
                if (!pixelComparer(self[x, y], other[x, y]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool IsEqualTo(this Image<RgbaVector> self, Image<RgbaVector> other, float tolerance = 1e-5f)
    {
        bool Comparer(RgbaVector p1, RgbaVector p2) =>
            MathF.Abs(p1.R - p2.R) <= tolerance &&
            MathF.Abs(p1.G - p2.G) <= tolerance &&
            MathF.Abs(p1.B - p2.B) <= tolerance &&
            MathF.Abs(p1.A - p2.A) <= tolerance;

        return self.IsEqualTo(other, Comparer);
    }

    /// <summary>
    /// Evaluates a single pixel using convolution.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static RgbaVector FilterOnePixel(this Image<RgbaVector> image, Filter filter, int x, int y)
    {
        static int Wrap(int n, int bound) => ((n % bound) + bound) % bound;

        int size = filter.KernelSize;
        int offset = size / 2;

        float r = 0, g = 0, b = 0, a = 0;

        for (int ky = 0; ky < size; ky++)
        {
            for (int kx = 0; kx < size; kx++)
            {
                int srcX = x + kx - offset;
                int srcY = y + ky - offset;

                switch (filter.EdgeMode)
                {
                    case EdgeMode.Clamp:
                        srcX = Math.Clamp(srcX, 0, image.Width - 1);
                        srcY = Math.Clamp(srcY, 0, image.Height - 1);
                        break;
                    case EdgeMode.Wrap:
                        srcX = Wrap(srcX, image.Width);
                        srcY = Wrap(srcY, image.Height);
                        break;
                }

                var pixel = image[srcX, srcY];
                float weight = filter.Kernel[ky, kx];

                r += pixel.R * weight;
                g += pixel.G * weight;
                b += pixel.B * weight;
                a += pixel.A * weight;
            }
        }

        r = (r * filter.Factor) + filter.Bias;
        g = (g * filter.Factor) + filter.Bias;
        b = (b * filter.Factor) + filter.Bias;
        a = (a * filter.Factor) + filter.Bias;

        return new RgbaVector(r, g, b, a);
    }
}
