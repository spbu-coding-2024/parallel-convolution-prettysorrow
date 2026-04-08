namespace Convolution.Extensions;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class FilterExtensions
{
    public static Rgb24 ApplyToRgb24(this Filter filter, Image<Rgb24> source, int x, int y)
    {
        int kSize = filter.Kernel.GetLength(0);
        int offset = kSize / 2;

        double r = 0, g = 0, b = 0;
        for (int ky = 0; ky < kSize; ky++)
        {
            for (int kx = 0; kx < kSize; kx++)
            {
                int srcX = x + kx - offset;
                int srcY = y + ky - offset;
                srcX = Math.Clamp(srcX, 0, source.Width - 1);
                srcY = Math.Clamp(srcY, 0, source.Height - 1);
                var pixel = source[srcX, srcY];
                double weight = filter.Kernel[ky, kx];
                r += pixel.R * weight;
                g += pixel.G * weight;
                b += pixel.B * weight;
            }
        }

        double factor = filter.Factor;
        double bias = filter.Bias;
        return new Rgb24(
            (byte)Math.Clamp((int)((factor * r) + bias), 0, 255),
            (byte)Math.Clamp((int)((factor * g) + bias), 0, 255),
            (byte)Math.Clamp((int)((factor * b) + bias), 0, 255)
        );
    }
}
