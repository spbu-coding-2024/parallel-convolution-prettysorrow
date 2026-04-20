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
        int batchesAmount = (source.Height + batchSize - 1) / batchSize; // ceiled (source.Height / batchSize)
        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startY = batchIndex * batchSize;
            int endY = Math.Min(startY + batchSize, source.Height);
            ApplyOneTile(source, result, filter, startY, endY, 0, source.Width);
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
        int batchesAmount = (source.Width + batchSize - 1) / batchSize; // ceiled (source.Width / batchSize)
        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startX = batchIndex * batchSize;
            int endX = Math.Min(startX + batchSize, source.Width);
            ApplyOneTile(source, result, filter, 0, source.Height, startX, endX);
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
        int tileSize_ = EstimateOptimalTileSize(source.Width, source.Height); // TODO: weird
        int tilesAcross = (source.Width + tileSize_ - 1) / tileSize_; // ceiled (source.Width / tileSize_)
        int tilesDown = (source.Height + tileSize_ - 1) / tileSize_;  // ceiled (source.Height / tileSize_)
        int totalTiles = tilesAcross * tilesDown;
        System.Threading.Tasks.Parallel.For(0, totalTiles, tileIndex =>
        {
            // left-to-right-top-to-bottom tiles traverse order
            int tileRow = tileIndex / tilesAcross;
            int tileColumn = tileIndex % tilesAcross;
            int startX = tileColumn * tileSize_;
            int endX = Math.Min(startX + tileSize_, source.Width);
            int startY = tileRow * tileSize_;
            int endY = Math.Min(startY + tileSize_, source.Height);
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
        return (int)Math.Max(16, Math.Sqrt(pixelsPerTile)); // TODO: is 16 OK?
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