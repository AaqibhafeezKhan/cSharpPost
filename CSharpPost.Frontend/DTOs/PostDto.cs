namespace CSharpPost.Frontend.DTOs
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto User { get; set; }
        public int Likes { get; set; }
        public List<string> Hashtags { get; set; } = new List<string>();
        public List<string> Mentions { get; set; } = new List<string>();
        public bool IsEdited { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserDto
    {
        public string Username { get; set; }
    }
}
