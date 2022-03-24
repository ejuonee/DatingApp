namespace DatingApp.Entities
{
  public class Connection
  {
    public Connection()
    {
    }

    public Connection(string connectionId, string username/*User user, int connectionId, User connectionUser */)
    {
      ConnectionId = connectionId;
      Username = username;
      // /User = user;
      // this.ConnectionId = connectionId;
      // ConnectionUser = connectionUser;
    }

    public string ConnectionId { get; set; }
    public string Username { get; set; }
    // public User User { get; set; }
    // public int ConnectionId { get; set; }
    // public User ConnectionUser { get; set; }
  }
}