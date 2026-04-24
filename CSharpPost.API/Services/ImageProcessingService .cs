using MicrobloggingApp.API.Services.Interfaces;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace MicrobloggingApp.API.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly IBlobStorageService _blobStorageService;

        public ImageProcessingService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task ProcessImageAsync(string originalImageUrl, string processedImageFileName)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(originalImageUrl);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to download image.");

            using var inputStream = await response.Content.ReadAsStreamAsync();
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream);

            image.Mutate(x => x.Resize(300, 300)); // Resize to predefined dimensions
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new WebpEncoder());
            outputStream.Position = 0;

            await _blobStorageService.UploadFileAsync(processedImageFileName, outputStream);
        }
    }
}
