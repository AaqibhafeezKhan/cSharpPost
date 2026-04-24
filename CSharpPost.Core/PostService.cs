using MicrobloggingApp.Core.DTOs;
using MicrobloggingApp.Data;
using MicrobloggingApp.Data.Interfaces;

namespace MicrobloggingApp.Core
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public void CreatePost(string text, string imagePath, int userId)
        {
            var post = new Post
            {
                Text = text,
                ImagePath = imagePath,
                Latitude = new Random().NextDouble() * 180 - 90,
                Longitude = new Random().NextDouble() * 360 - 180,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _postRepository.Add(post);
        }

        public IEnumerable<PostDto> GetTimeline()
        {
            return _postRepository.GetAll()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDto
                {
                    Text = p.Text,
                    ImagePath = p.ImagePath.Contains("processed")
                        ? p.ImagePath
                        : $"original-{p.ImagePath}", // Use processed if available, else fallback
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    CreatedAt = p.CreatedAt
                });
        }

        public Post? GetPostById(int id)
        {
            return _postRepository.GetById(id);
        }

        public void UpdatePost(Post post)
        {
            _postRepository.Update(post);
        }

        public void DeletePost(Post post)
        {
            _postRepository.Delete(post);
        }
    }
}
