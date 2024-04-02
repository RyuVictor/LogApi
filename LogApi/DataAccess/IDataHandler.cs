using LogApi.Models;
using System.Globalization;

namespace LogApi.DataAccess
{
    public interface IDataHandler
    {
        public void AddException(MyException exception);
        public List<MyException> GetAllExceptions();
        public void DeleteException(int id);
        public List<GroupCount> GroupExceptionsByProperty(string propertyName);
        public List<MyException> FilterExceptionsByProperty(List<MyException> exceptions, string propertyName, string propertyValue);
        public List<MyException> GetRecentExceptions();
    }
}
