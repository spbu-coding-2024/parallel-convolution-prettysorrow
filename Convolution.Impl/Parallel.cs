namespace Convolution.Impl;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Provides parallel image convolution implementations.
/// </summary>
public static class Parallel
{
    /// <summary>
    /// Applies a convolution filter to an image using row-by-row parallel processing.
    /// </summary>
    public static Image<RgbaVector> Apply(Image<RgbaVector> source, Filter filter)
        => ApplyRows(source, filter);

    /// <summary>
    /// Applies a convolution filter to an image using row-by-row parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyRows(Image<RgbaVector> source, Filter filter)
    {
        Image<RgbaVector> result = new(source.Width, source.Height);

        int batchSize = Math.Max(1, source.Height / Environment.ProcessorCount);
        int batchesAmount = (source.Height + batchSize - 1) / batchSize;

        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startY = batchIndex * batchSize;
            int endY = Math.Min(startY + batchSize, source.Height);

            int startX = 0;
            int endX = source.Width;

            ApplyOneTile(source, result, filter, startY, endY, startX, endX);
        });

        return result;
    }

    /// <summary>
    /// Applies a convolution filter to an image using column-by-column parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyColumns(Image<RgbaVector> source, Filter filter)
    {
        Image<RgbaVector> result = new(source.Width, source.Height);

        int batchSize = Math.Max(1, source.Width / Environment.ProcessorCount);
        int batchesAmount = (source.Width + batchSize - 1) / batchSize;

        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startY = 0;
            int endY = source.Height;

            int startX = batchIndex * batchSize;
            int endX = Math.Min(startX + batchSize, source.Width);

            ApplyOneTile(source, result, filter, startY, endY, startX, endX);
        });

        return result;
    }

    /// <summary>
    /// Applies a convolution filter to an image using tiled parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyTiles(Image<RgbaVector> source, Filter filter, int tileSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileSize);

        Image<RgbaVector> result = new(source.Width, source.Height);

        int tilesAcross = (source.Width + tileSize - 1) / tileSize;
        int tilesDown = (source.Height + tileSize - 1) / tileSize;
        int tilesTotal = tilesAcross * tilesDown;

        // left-to-right-top-to-bottom tiles traverse order
        System.Threading.Tasks.Parallel.For(0, tilesTotal, tileIndex =>
        {
            int tileRow = tileIndex / tilesAcross;
            int startY = tileRow * tileSize;
            int endY = Math.Min(startY + tileSize, source.Height);

            int tileColumn = tileIndex % tilesAcross;
            int startX = tileColumn * tileSize;
            int endX = Math.Min(startX + tileSize, source.Width);

            ApplyOneTile(source, result, filter, startY, endY, startX, endX);
        });

        return result;
    }

    /// <summary>
    /// Applies a convolution filter to an image using tiled parallel processing.
    /// </summary>
    public static Image<RgbaVector> ApplyTiles(Image<RgbaVector> source, Filter filter)
    {
        int tileSize = EstimateOptimalTileSize(source.Width, source.Height);
        return ApplyTiles(source, filter, tileSize);
    }

    private static int EstimateOptimalTileSize(int width, int height)
    {
        int totalPixels = width * height;
        int pixelsPerTile = totalPixels / Environment.ProcessorCount;
        return (int)Math.Max(16, Math.Sqrt(pixelsPerTile));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void ApplyOneTile(
        Image<RgbaVector> source,
        Image<RgbaVector> destination,
        Filter filter,
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