namespace MicrobloggingApp.Frontend.DTOs
{
    public class PaginatedResponse
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public List<PostDto> Posts { get; set; }
    }
}
