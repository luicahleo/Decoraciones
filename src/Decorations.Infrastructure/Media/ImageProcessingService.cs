using Decorations.Application.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Decorations.Infrastructure.Media
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ImageProcessingOptions options;

        public ImageProcessingService(IOptions<ImageProcessingOptions> options)
        {
            this.options = options.Value;
        }

        public async Task<byte[]> ProcessImageAsync(Stream imageStream, string originalFileName)
        {
            if (imageStream.CanSeek && imageStream.Length > this.options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    $"El archivo supera el tamaño máximo de {this.options.MaxFileSizeBytes / 1048576} MB.");
            }

            using Image image = await Image.LoadAsync(imageStream);

            bool needsResize = image.Width > this.options.MaxWidthPixels || image.Height > this.options.MaxHeightPixels;
            if (needsResize)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(this.options.MaxWidthPixels, this.options.MaxHeightPixels),
                    Mode = ResizeMode.Max
                }));
            }

            using MemoryStream outputStream = new MemoryStream();
            WebpEncoder encoder = new WebpEncoder { Quality = this.options.WebPQuality };
            await image.SaveAsync(outputStream, encoder);
            return outputStream.ToArray();
        }
    }
}
