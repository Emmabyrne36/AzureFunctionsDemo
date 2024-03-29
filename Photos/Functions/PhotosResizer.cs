using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Photos.Enums;

namespace Photos.Functions
{
    public partial class PhotosResizer
    {
        // TODO: Uncomment when local storage set up
        [FunctionName(FunctionNames.PhotosResizer)]
        public static async Task Run(
            [BlobTrigger("photos/{name}", Connection = Literals.StorageConnectionString)] Stream myBlob,
            [Blob("photos-small/{name}", FileAccess.Write, Connection = Literals.StorageConnectionString)] Stream imageSmall,
            [Blob("photos-medium/{name}", FileAccess.Write, Connection = Literals.StorageConnectionString)] Stream imageMedium,
            ILogger logger)
        {
            logger?.LogInformation("Resizing photo...");

            try
            {
                await Resize(myBlob, imageMedium, ImageSize.Medium);

                await Resize(myBlob, imageSmall, ImageSize.Small);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
            }
            finally
            {
                imageSmall.Close();
                imageMedium.Close();
            }
        }

        private static MemoryStream CreateMemoryStream(Stream image, ImageSize imageSize)
        {
            var ms = new MemoryStream();
            var img = Image.FromStream(image);
            var desiredWidth = imageSize == ImageSize.Medium ? img.Width / 2 :
                                                        img.Width / 4;
            var ratio = (decimal)desiredWidth / img.Width;
            var resized = ResizeImage(img, desiredWidth, (int)Math.Floor((img.Height * ratio)));
            resized.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            return ms;
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static async Task Resize(Stream myBlob, Stream image, ImageSize imageSize)
        {
            var memoryStream = CreateMemoryStream(myBlob, imageSize);
            await memoryStream.CopyToAsync(image);
        }
    }
}
