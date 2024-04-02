using LogApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace LogApi.DataAccess.Exceptions
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
