namespace MicrobloggingApp.API.DTOs
{
    public class CreatePostRequest
    {
        public string Text { get; set; }
        public IFormFile Image { get; set; }
    }
}
