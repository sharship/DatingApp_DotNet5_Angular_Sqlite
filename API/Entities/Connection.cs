namespace API.Entities
{
    public class Connection
    {
        public Connection()
        {
        }

        public Connection(string connectionId, string username)
        {
            ConnectionId = connectionId;

            // user who connects to hub
            Username = username;

        }
        public string ConnectionId { get; set; }

        public string Username { get; set; }


    }
}