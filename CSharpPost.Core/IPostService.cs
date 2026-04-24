using MicrobloggingApp.Core.DTOs;
using MicrobloggingApp.Data;

namespace MicrobloggingApp.Core
{
    public interface IPostService
    {
        void CreatePost(string text, string imagePath, int userId);
        IEnumerable<PostDto> GetTimeline();
        Post? GetPostById(int id);
        void UpdatePost(Post post);
        void DeletePost(Post post);
    }
}
