namespace LogApi.Models
{
    public class MyException
    {
        public int Id { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

    }
}
