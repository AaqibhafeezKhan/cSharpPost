namespace MicrobloggingApp.Core
{
    public interface IUserService
    {
        bool ValidateUser(string username, string password);
    }
}
