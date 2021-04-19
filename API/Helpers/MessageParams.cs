namespace API.Helpers
{
    public class MessageParams : PaginationParams
    {
        public string CurrentUsername { get; set; }
        
        public string Container { get; set; } = "Unread";
        
    }
}