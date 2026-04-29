namespace Convolution.Impl;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Provides parallel image convolution implementations.
/// </summary>
public static class Parallel
{
    /// <summary>
    /// Applies a convolution filter to an image.
    /// </summary>
    public static Image<RgbaVector> Apply(this Filter filter, Image<RgbaVector> image)
        => filter.ApplyRows(image);

    /// <summary>
    /// Filters an image using specified convolution filter.
    /// </summary>
    public static Image<RgbaVector> Filter(this Image<RgbaVector> image, Filter filter)
        => filter.ApplyRows(image);

    /// <summary>
    /// Applies a convolution filter to an image using row-by-row parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyRows(this Filter filter, Image<RgbaVector> image)
    {
        Image<RgbaVector> result = new(image.Width, image.Height);

        int batchSize = Math.Max(1, image.Height / Environment.ProcessorCount);
        int batchesAmount = (image.Height + batchSize - 1) / batchSize;

        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startY = batchIndex * batchSize;
            int endY = Math.Min(startY + batchSize, image.Height);

            int startX = 0;
            int endX = image.Width;

            filter.ApplyOneTile(image, result, startY, endY, startX, endX);
        });

        return result;
    }

    /// <summary>
    /// Applies a convolution filter to an image using column-by-column parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyColumns(this Filter filter, Image<RgbaVector> image)
    {
        Image<RgbaVector> result = new(image.Width, image.Height);

        int batchSize = Math.Max(1, image.Width / Environment.ProcessorCount);
        int batchesAmount = (image.Width + batchSize - 1) / batchSize;

        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startY = 0;
            int endY = image.Height;

            int startX = batchIndex * batchSize;
            int endX = Math.Min(startX + batchSize, image.Width);

            filter.ApplyOneTile(image, result, startY, endY, startX, endX);
        });

        return result;
    }

    /// <summary>
    /// Applies a convolution filter to an image using tiled parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyTiles(this Filter filter, Image<RgbaVector> image, int tileSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileSize);

        Image<RgbaVector> result = new(image.Width, image.Height);

        int tilesAcross = (image.Width + tileSize - 1) / tileSize;
        int tilesDown = (image.Height + tileSize - 1) / tileSize;
        int tilesTotal = tilesAcross * tilesDown;

        // left-to-right-top-to-bottom tiles traverse order
        System.Threading.Tasks.Parallel.For(0, tilesTotal, tileIndex =>
        {
            int tileRow = tileIndex / tilesAcross;
            int startY = tileRow * tileSize;
            int endY = Math.Min(startY + tileSize, image.Height);

            int tileColumn = tileIndex % tilesAcross;
            int startX = tileColumn * tileSize;
            int endX = Math.Min(startX + tileSize, image.Width);

            filter.ApplyOneTile(image, result, startY, endY, startX, endX);
        });

        return result;
    }

    /// <summary>
    /// Applies a convolution filter to an image using tiled parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyTiles(this Filter filter, Image<RgbaVector> image)
    {
        int tileSize = EstimateOptimalTileSize(image.Width, image.Height);
        return filter.ApplyTiles(image, tileSize);
    }

    private static int EstimateOptimalTileSize(int width, int height)
    {
        int totalPixels = width * height;
        int pixelsPerTile = totalPixels / Environment.ProcessorCount;
        return (int)Math.Max(16, Math.Sqrt(pixelsPerTile));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void ApplyOneTile(
        this Filter filter,
        Image<RgbaVector> source,
        Image<RgbaVector> destination,
        int startY,
        int endY,
        int startX,
        int endX)
    {
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                destination[x, y] = source.FilterOnePixel(filter, x, y);
            }
        }
    }
}
