namespace MicrobloggingApp.Core
{
    public class UserService : IUserService
    {
        private readonly Dictionary<string, string> _users = new()
        {
            { "user1", "password1" },
            { "user2", "password2" }
        };

        public bool ValidateUser(string username, string password)
        {
            return _users.TryGetValue(username, out var storedPassword) && storedPassword == password;
        }
    }
}
