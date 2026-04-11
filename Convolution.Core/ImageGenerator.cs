namespace Convolution.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using System.Security.Cryptography.X509Certificates;

public class ImageGenerator(int? seed = null)
{
    private readonly Random _random = seed.HasValue ? new Random(seed.Value) : new Random();

    public Image<Rgb24> Next(int width = 800, int height = 600, int shapeCount = 20)
    {
        var image = new Image<Rgb24>(width, height);

        var backgroundColor = new Rgb24(
            (byte)this._random.Next(256),
            (byte)this._random.Next(256),
            (byte)this._random.Next(256)
        );

        image.Mutate(ctx => ctx.BackgroundColor(backgroundColor));

        for (int i = 0; i < shapeCount; i++)
            this.DrawRandomShape(image);

        return image;
    }

    private void DrawRandomShape(Image<Rgb24> image)
    {
        var color = new Rgb24(
            (byte)this._random.Next(256),
            (byte)this._random.Next(256),
            (byte)this._random.Next(256)
        );

        var (x, y) = (this._random.Next(image.Width), this._random.Next(image.Height));

        image.Mutate(ctx =>
        {
            switch (this._random.Next(3))
            {
                case 0:
                    var rectangle = new RectangleF(
                        x,
                        y,
                        width: this._random.Next(20, 200),
                        height: this._random.Next(20, 200)
                    );

                    ctx.Fill(color, rectangle);
                    break;

                case 1:
                    var ellipse = new EllipsePolygon(
                        x,
                        y,
                        radius: this._random.Next(10, 150)
                    );

                    ctx.Fill(color, ellipse);
                    break;

                case 2:
                    var triangle = new RegularPolygon(
                        x,
                        y,
                        vertices: 3,
                        radius: this._random.Next(10, 150)
                    );

                    ctx.Fill(color, triangle);
                    break;
            }
        });
    }
}