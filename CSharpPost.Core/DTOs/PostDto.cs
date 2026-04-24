namespace MicrobloggingApp.Core.DTOs
{
    public class PostDto
    {
        public string Text { get; set; }
        public string ImagePath { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
