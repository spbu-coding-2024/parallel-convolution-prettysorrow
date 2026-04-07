namespace Convolution.Impl;

using Convolution.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class Parallel
{
    public static Image<Rgb24> Apply(Image<Rgb24> source, Filter filter)
    {
        int w = source.Width;
        int h = source.Height;
        double[,] kernel = filter.Kernel;
        int kSize = kernel.GetLength(0);
        int offset = kSize / 2;
        double factor = filter.Factor;
        double bias = filter.Bias;

        var result = new Image<Rgb24>(w, h);

        System.Threading.Tasks.Parallel.For(0, h, y =>
        {
            for (int x = 0; x < w; x++)
            {
                double r = 0, g = 0, b = 0;

                for (int ky = 0; ky < kSize; ky++)
                {
                    for (int kx = 0; kx < kSize; kx++)
                    {
                        int srcX = x + kx - offset;
                        int srcY = y + ky - offset;

                        srcX = Math.Clamp(srcX, 0, w - 1);
                        srcY = Math.Clamp(srcY, 0, h - 1);

                        var pixel = source[srcX, srcY];
                        double weight = kernel[ky, kx];

                        r += pixel.R * weight;
                        g += pixel.G * weight;
                        b += pixel.B * weight;
                    }
                }

                result[x, y] = new Rgb24(
                    (byte)System.Math.Clamp((int)((factor * r) + bias), 0, 255),
                    (byte)System.Math.Clamp((int)((factor * g) + bias), 0, 255),
                    (byte)System.Math.Clamp((int)((factor * b) + bias), 0, 255)
                );
            }
        });

        return result;
    }
}