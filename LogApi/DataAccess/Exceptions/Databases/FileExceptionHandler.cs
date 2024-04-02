using System;
using System.Globalization;
using System.IO;
using LogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LogApi.DataAccess.Exceptions.Databases
{
    public class FileExceptionHandler : IDataHandler
    {
        private readonly string _logFilePath;

        public FileExceptionHandler(string logFilePath)
        {
            _logFilePath = logFilePath;
        }


        public void DeleteException(int id)
        {
            throw new NotImplementedException();
        }

        public List<MyException> GetAllExceptions()
        {
            var exceptions = new List<MyException>();

            try
            {
                foreach (string line in File.ReadLines(_logFilePath))
                {
                    var exception = JsonConvert.DeserializeObject<MyException>(line);
                    exceptions.Add(exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return exceptions;
        }
        public void AddException(MyException exception)
        {
            var logEntry = JsonConvert.SerializeObject(exception);

            using (StreamWriter writer = File.AppendText(_logFilePath))
            {
                writer.WriteLine(logEntry);
            }
        }
        public List<GroupCount> GroupExceptionsByProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        public List<MyException> FilterExceptionsByProperty(List<MyException> exceptions,string propertyName, string propertyValue)
        {
            throw new NotImplementedException();
        }

        public List<MyException> GetRecentExceptions()
        {
            throw new NotImplementedException();
        }
    }
}
