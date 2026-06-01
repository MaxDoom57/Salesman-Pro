namespace Bridge_App.Models
{
    public class SessionData
    {
        public int CompanyKey { get; set; }
        public int ProjectKey { get; set; }
        public string UserId { get; set; }
        public string ConnectionString { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
