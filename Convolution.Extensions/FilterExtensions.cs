namespace Convolution.Extensions;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class FilterExtensions
{
    public static RgbaVector FilterOnePixel(this Image<RgbaVector> source, Filter filter, int x, int y)
    {
        int kSize = filter.Kernel.GetLength(0);
        int offset = kSize / 2;
        float r = 0, g = 0, b = 0;

        for (int ky = 0; ky < kSize; ky++)
        {
            for (int kx = 0; kx < kSize; kx++)
            {
                int srcX = x + kx - offset;
                int srcY = y + ky - offset;

                switch (filter.edgeMode)
                {
                    case Filter.EdgeMode.Clamp:
                        srcX = Math.Clamp(srcX, 0, source.Width - 1);
                        srcY = Math.Clamp(srcY, 0, source.Height - 1);
                        break;
                    case Filter.EdgeMode.Wrap:
                        srcX = ((srcX % source.Width) + source.Width) % source.Width;
                        srcY = ((srcY % source.Height) + source.Height) % source.Height;
                        break;
                }

                var pixel = source[srcX, srcY];
                float weight = (float)filter.Kernel[ky, kx];
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
