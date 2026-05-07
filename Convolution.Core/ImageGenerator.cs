namespace Convolution.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;

/// <summary>
/// Generates random images.
/// </summary>
public class ImageGenerator(int? seed = null)
{
    private readonly Random random = seed.HasValue ? new Random(seed.Value) : new Random();

    public Image<RgbaVector> Next(int width = 200, int height = 150, int shapeCount = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));
        ArgumentOutOfRangeException.ThrowIfNegative(shapeCount, nameof(shapeCount));

        var image = new Image<RgbaVector>(width, height);
        image.Mutate(ctx => ctx.BackgroundColor(this.NextColor()));
        for (int i = 0; i < shapeCount; i++)
        {
            this.DrawRandomShape(image);
        }

        return image;
    }

    private Color NextColor()
        => Color.FromRgb(
            (byte)this.random.Next(256),
            (byte)this.random.Next(256),
            (byte)this.random.Next(256));

    private void DrawRandomShape(Image<RgbaVector> image)
    {
        var color = this.NextColor();
        var (x, y) = (this.random.Next(image.Width), this.random.Next(image.Height));
        image.Mutate(ctx =>
        {
            switch (this.random.Next(3))
            {
                case 0:
                    ctx.Fill(color, new RectangleF(
                        x,
                        y,
                        width: this.random.Next(20, 200),
                        height: this.random.Next(20, 200)));
                    break;
                case 1:
                    ctx.Fill(color, new EllipsePolygon(
                        x,
                        y,
                        radius: this.random.Next(20, 200)));
                    break;
                case 2:
                    ctx.Fill(color, new RegularPolygon(
                        x,
                        y,
                        vertices: 3,
                        radius: this.random.Next(20, 200)));
                    break;
            }
        });
    }

    public List<string> WriteRandomImages(string destDir, int count = 10, int width = 200, int height = 100)
    {
        if (!Directory.Exists(destDir))
        {
            throw new DirectoryNotFoundException(nameof(destDir));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(count));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));

        List<string> result = new(count);

        for (int i = 0; i < count; ++i)
        {
            using var image = this.Next(width, height);
            string imagePath = System.IO.Path.Combine(destDir, $"{Guid.NewGuid()}.png");
            image.Save(imagePath);
            result.Add(imagePath);
        }

        return result;
    }
}
