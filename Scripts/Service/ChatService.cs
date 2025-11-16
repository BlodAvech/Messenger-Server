public class ChatService
{
    private readonly DatabaseManager _db;
    public ChatService(DatabaseManager db) => _db = db;

    public User LoginUser(User user) => _db.LoginUser(user.UserName , user.Password);
    public User RegisterUser(User user) => _db.RegisterUser(user.UserName , user.Password);

    public void SendMessage(Message msg) => _db.SendMessage(msg.ChatId , msg.User.UserId , msg.Text);
    public void SendExpiringMessage(Message msg , int seconds) => _db.SendMessage(msg.ChatId , msg.User.UserId , msg.Text , seconds);
    
    public void DeleteMessage(Message msg , User user) => _db.DeleteMessage(msg.MessageId , user.UserId);

    public Chat CreateChat(Chat chat) => _db.CreateChat(chat.ChatName , chat.OwnerId  , chat.Type);
    public Chat CreatePrivateChat(int user1 ,  int user2) => _db.CreatePrivateChat(user1 , user2);
    public Chat JoinChat(Chat chat , User user) => _db.JoinChat(chat.ChatId , user.UserId);
    public void AddUserToGroup(Chat chat , int owner , int user) => _db.AddUserToGroup(chat.ChatId , owner , user);
    public void LeaveChat(Chat chat , User user) => _db.LeaveChat(chat.ChatId , user.UserId);
    public void ClearChat(Chat chat , User user) => _db.ClearChat(chat.ChatId , user.UserId);
    public void DeleteChat(Chat chat , User user) => _db.DeleteChat(chat.ChatId , user.UserId);
    public Chat GetChat(int chatId) => _db.GetChat(chatId);

    public List<User> GetChatMembers(Chat chat) => _db.GetChatMembers(chat.ChatId);
    public List<Message> GetMessages(Chat chat) => _db.GetMessages(chat.ChatId);

    public List<Chat> GetChats(int user) => _db.GetChatsOfUser(user);
}
