using LogApi.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LogApi.DataAccess.Exceptions.Databases
{
    public class DataHandlerMongo : IDataHandler
    {
        private readonly string _connectionString;
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _exceptionsCollection;

        public DataHandlerMongo(string connectionString)
        {
            _connectionString = connectionString;
            _client = new MongoClient(_connectionString);
            _database = _client.GetDatabase("LogDb");
            _exceptionsCollection = _database.GetCollection<BsonDocument>("Exceptions");
        }

        public void AddException(MyException exception)
        {
            var exceptionDoc = new BsonDocument
            {
                { "statusCode", exception.StatusCode },
                { "message", exception.Message },
                { "stackTrace", exception.StackTrace },
                { "source", exception.Source },
                { "severity", exception.Severity },
                { "applicationName", exception.ApplicationName },
                { "timestamp", exception.Timestamp }
            };

            _exceptionsCollection.InsertOne(exceptionDoc);
        }

        public void DeleteException(int id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            _exceptionsCollection.DeleteOne(filter);
        }

        public List<MyException> FilterExceptionsByProperty(List<MyException> exceptions, string propertyName, string propertyValue)
        {
            if (exceptions.Count == 0)
            {
                exceptions = GetAllExceptions();
            }

            var filteredExceptions = exceptions.Where(e => MatchPropertyValue(e, propertyName, propertyValue)).ToList();
            Console.WriteLine(filteredExceptions.Count + " this is the count");
            return filteredExceptions;
        }

        private bool MatchPropertyValue(MyException exception, string propertyName, string propertyValue)
        {
            switch (propertyName.ToLower())
            {
                case "statuscode":
                    return exception.StatusCode.ToString().Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "message":
                    return exception.Message.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "stacktrace":
                    return exception.StackTrace.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "source":
                    return exception.Source.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "severity":
                    return exception.Severity.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "timestamp":
                    return exception.Timestamp.ToString().Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "applicationname":
                    return exception.ApplicationName.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }

        public List<MyException> GetAllExceptions()
        {
            var exceptions = new List<MyException>();

            var exceptionDocs = _exceptionsCollection.Find(new BsonDocument()).ToList();

            foreach (var doc in exceptionDocs)
            {
                var exception = new MyException
                {
                    Id = doc.GetValue("_id").AsObjectId.GetHashCode(), // Explicitly convert ObjectId to int
                    StatusCode = doc.GetValue("statusCode").AsInt32,
                    Message = doc.GetValue("message").AsString,
                    StackTrace = doc.GetValue("stackTrace").AsString,
                    Source = doc.GetValue("source").AsString,
                    Severity = doc.GetValue("severity").AsString,
                    ApplicationName = doc.GetValue("applicationName").AsString,
                    Timestamp = doc.GetValue("timestamp").ToUniversalTime()
                };
                exceptions.Add(exception);
            }

            return exceptions;
        }


        public List<MyException> GetRecentExceptions()
        {
            var exceptions = GetAllExceptions();
            var recentExceptions = exceptions.OrderBy(e => e.Timestamp).Take(10).ToList();
            return recentExceptions;
        }

        public List<GroupCount> GroupExceptionsByProperty(string propertyName)
        {
            var propertyCounts = new List<GroupCount>();

            var groupField = propertyName == "statuscode" ? "$statusCode" : propertyName == "applicationname" ? "$applicationName" : "$source";

            var groupStage = new BsonDocument
            {
                { "_id", groupField },
                { "ExceptionCount", new BsonDocument("$sum", 1) }
            };

            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$group", groupStage)
            };

            var result = _exceptionsCollection.Aggregate<BsonDocument>(pipeline).ToList();

            foreach (var doc in result)
            {
                var propertyCount = new GroupCount
                {
                    GroupName = doc.GetValue("_id").IsString ? doc.GetValue("_id").AsString : doc.GetValue("_id").ToString(),
                    Count = doc.GetValue("ExceptionCount").AsInt32
                };
                propertyCounts.Add(propertyCount);
            }

            return propertyCounts;
        }
    }
}
