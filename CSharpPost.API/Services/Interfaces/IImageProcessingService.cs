namespace MicrobloggingApp.API.Services.Interfaces
{
    public interface IImageProcessingService
    {
        Task ProcessImageAsync(string originalImageUrl, string processedImageFileName);
    }
}
