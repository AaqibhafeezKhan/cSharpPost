namespace MicrobloggingApp.API.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(string fileName, Stream fileStream);
    }
}
