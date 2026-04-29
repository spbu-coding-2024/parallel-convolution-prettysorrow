namespace Convolution.Impl;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Provides sequential image convolution implementation.
/// </summary>
public static class Sequential
{
    /// <summary>
    /// Applies a convolution filter to an image.
    /// </summary>
    public static Image<RgbaVector> Apply(this Filter filter, Image<RgbaVector> image)
    {
        var result = new Image<RgbaVector>(image.Width, image.Height);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                result[x, y] = image.FilterOnePixel(filter, x, y);
            }
        }

        return result;
    }

    /// <summary>
    /// Filters an image using specified convolution filter.
    /// </summary>
    public static Image<RgbaVector> Filter(this Image<RgbaVector> image, Filter filter)
        => Apply(filter, image);
}
