namespace LogApi.Models
{
    public class LoggedInUser
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }
        public string JobTitle { get; set; }
        public string UserPrincipalName { get; set; }
        public string MobileNo { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
