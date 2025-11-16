public class Message
{
    public int MessageId { get; set; }
    public int ChatId { get; set; }
    public User User { get; set; }
    public string Text { get; set; }
    public DateTime? CreatedAt { get; set; } 
    public int ExpiringTime  { get; set; } = 0;
}

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string? Password { get; set; } 
}

public class Chat
{
    public int ChatId { get; set; }
    public string ChatName { get; set; }
    public int Type { get; set; } 
    public int OwnerId { get; set; }
}

public class AddUserRequest
{
    public int Owner { get; set; }
    public int NewUser { get; set; }
}