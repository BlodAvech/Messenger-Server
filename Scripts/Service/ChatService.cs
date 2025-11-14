public class ChatService
{
    private readonly DatabaseManager _db;
    public ChatService(DatabaseManager db) => _db = db;

    public void SendMessage(SendMessageDto msgDto) => _db.SendMessage(msgDto.ChatId , msgDto.UserId , msgDto.MessageText);
    public int LoginUser(UserDto userDto) => _db.LoginUser(userDto.Name , userDto.Password);
    public int RegisterUser(UserDto userDto) => _db.RegisterUser(userDto.Name , userDto.Password);
}
