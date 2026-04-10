namespace Convolution.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;

public class ImageGenerator(int? seed = null)
{
    private readonly Random _random = seed.HasValue ? new Random(seed.Value) : new Random();

    public Image<Rgb24> Generate(int width = 800, int height = 600, int shapeCount = 20)
    {
        var image = new Image<Rgb24>(width, height);

        var backgroundColor = new Rgb24(
            (byte)this._random.Next(256),
            (byte)this._random.Next(256),
            (byte)this._random.Next(256)
        );

        image.Mutate(ctx => ctx.BackgroundColor(backgroundColor));

        for (int i = 0; i < shapeCount; i++)
            this.DrawRandomShape(image, width, height);

        return image;
    }

    private void DrawRandomShape(Image<Rgb24> image, int maxWidth, int maxHeight)
    {

        var color = new Rgb24(
            (byte)this._random.Next(256),
            (byte)this._random.Next(256),
            (byte)this._random.Next(256)
        );

        var shapeType = this._random.Next(3);

        image.Mutate(ctx =>
        {
            switch (shapeType)
            {
                case 0:
                    var rect = new RectangleF(
                        this._random.Next(maxWidth),
                        this._random.Next(maxHeight),
                        this._random.Next(20, 200),
                        this._random.Next(20, 200)
                    );

                    ctx.Fill(color, rect);
                    break;

                case 1:
                    var ellipse = new EllipsePolygon(
                        this._random.Next(maxWidth),
                        this._random.Next(maxHeight),
                        this._random.Next(10, 150)
                    );

                    ctx.Fill(color, ellipse);
                    break;

                case 2:
                    var pen = new SolidPen(color, this._random.Next(1, 5));
                    var points = new PointF[]
                    {
                        new (this._random.Next(maxWidth), this._random.Next(maxHeight)),
                        new (this._random.Next(maxWidth), this._random.Next(maxHeight))
                    };
                    ctx.DrawLine(pen, points);
                    break;
            }
        });
    }
}