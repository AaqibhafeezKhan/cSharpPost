namespace MicrobloggingApp.Data.Interfaces
{
    public interface IPostRepository
    {
        void Add(Post post);
        IEnumerable<Post> GetAll();
        Post? GetById(int id);
        void Update(Post post);
        void Delete(Post post);
    }
}
