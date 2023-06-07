namespace API.Entities
{
    public class Connection
    {
        // To satisfy Entity framework, when it creates the schema for our db
        // we have to give this an empty constructor so that when it does create
        // this new connection it's not expecting to also pass the connectionId as well.
        public Connection()
        {
        }

        public Connection(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;
        }

        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}
