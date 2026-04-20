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

    /// <summary>
    /// Generates a random image with specified number of random shapes.
    /// </summary>
    public Image<RgbaVector> Next(int width = 400, int height = 300, int shapeCount = 10)
    {
        var image = new Image<RgbaVector>(width, height);
        image.Mutate(ctx => ctx.BackgroundColor(this.NextColor()));
        for (int i = 0; i < shapeCount; i++)
        {
            this.DrawRandomShape(image);
        }

        return image;
    }

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
                        radius: this.random.Next(10, 150)));
                    break;
                case 2:
                    ctx.Fill(color, new RegularPolygon(
                        x,
                        y,
                        vertices: 3,
                        radius: this.random.Next(10, 150)));
                    break;
            }
        });
    }
}