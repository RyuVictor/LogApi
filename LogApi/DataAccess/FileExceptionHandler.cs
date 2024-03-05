using System;
using System.Globalization;
using System.IO;
using LogApi.Models;
using Newtonsoft.Json;


namespace LogApi.DataAccess
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
                // Handle exceptions, e.g., log or throw
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
    }
}
