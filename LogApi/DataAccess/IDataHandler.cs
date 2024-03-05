using LogApi.Models;

namespace LogApi.DataAccess
{
    public interface IDataHandler
    {
        public void AddException(MyException exception);
        public List<MyException> GetAllExceptions();
        public void DeleteException(int id);
    }
}
