
public class SendMessageDto
{
    public int UserId { get; set; }
    public int ChatId { get; set; }
    public string MessageText { get; set; }
}


public class UserDto
{
	public string Name{get;set;}
    public string Password{get;set;}
}