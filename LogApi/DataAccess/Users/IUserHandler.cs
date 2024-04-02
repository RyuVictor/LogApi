using LogApi.Models;

namespace LogApi.DataAccess.Users
{
    public interface IUserHandler
    {
        public void AddUser(LoggedInUser user);
        public List<LoggedInUser> GetAllUsers();
        public void DeleteUser(int id);
    }
}
