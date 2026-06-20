using Decorations.Application.DTOs;
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

        public async Task<ProcessedImageResult> ProcessImageAsync(Stream imageStream, string originalFileName)
        {
            if (imageStream.CanSeek && imageStream.Length > this.options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    $"El archivo supera el tamaño máximo de {this.options.MaxFileSizeBytes / 1048576} MB.");
            }

            using Image image = await Image.LoadAsync(imageStream);

            // Procesar thumbnail (clonar la imagen antes de modificarla)
            var thumbnailImage = image.Clone(c => { });
            byte[] thumbnailBytes = await this.ProcessImageVersionAsync(
                thumbnailImage,
                this.options.Thumbnail.MaxWidthPixels,
                this.options.Thumbnail.MaxHeightPixels,
                this.options.Thumbnail.WebPQuality);

            // Procesar full-size (usa la imagen original)
            byte[] fullSizeBytes = await this.ProcessImageVersionAsync(
                image,
                this.options.FullSize.MaxWidthPixels,
                this.options.FullSize.MaxHeightPixels,
                this.options.FullSize.WebPQuality);

            return new ProcessedImageResult
            {
                ThumbnailBytes = thumbnailBytes,
                FullSizeBytes = fullSizeBytes
            };
        }

        private async Task<byte[]> ProcessImageVersionAsync(Image image, int maxWidth, int maxHeight, int quality)
        {
            bool needsResize = image.Width > maxWidth || image.Height > maxHeight;
            if (needsResize)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max
                }));
            }

            using MemoryStream outputStream = new MemoryStream();
            WebpEncoder encoder = new WebpEncoder { Quality = quality };
            await image.SaveAsync(outputStream, encoder);
            return outputStream.ToArray();
        }
    }
}
