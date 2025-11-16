public class ChatService : IChatService
{
    private readonly DatabaseManager _db;
    public ChatService(DatabaseManager db) => _db = db;

    // Реализация всех событий и методов интерфейса
    public event UserEventHandler OnUserRegistred;
    public event UserEventHandler OnUserLogined;
    public event ChatEventHandler OnChatCreated;
    public event ChatEventHandler OnPrivateCreated;
    public event MessageEventHandler OnMessageSended;
    public event MessageEventHandler OnMessageDeleted;
    public event MessageEventHandler OnExpiringMessageSended; 
    public event MessageEventHandler OnExpiringMessageDeleted;
    public event ChatEventHandler OnJoinToChat;
    public event ChatEventHandler OnAddToChat;    public User LoginUser(User user)
    {
        User newUser = _db.LoginUser(user.UserName, user.Password);
        OnUserLogined?.Invoke(newUser);
        return newUser;
    }

    public User RegisterUser(User user)
    {
        User newUser = _db.RegisterUser(user.UserName, user.Password);
        OnUserRegistred?.Invoke(newUser);
        return newUser;
    }

    public void SendMessage(Message msg)
    {
        OnMessageSended?.Invoke(msg, msg.User);
        _db.SendMessage(msg.ChatId, msg.User.UserId, msg.Text);
    }

    public async Task SendExpiringMessage(Message msg, int seconds)
    {
        OnExpiringMessageSended?.Invoke(msg, seconds: seconds);
        int msgId = _db.SendMessage(msg.ChatId, msg.User.UserId, msg.Text);

        await Task.Delay(seconds * 1000);

        _db.DeleteMessage(msgId, msg.User.UserId);

        OnExpiringMessageDeleted?.Invoke(msg); 
    }

    public void DeleteMessage(Message msg, User user)
    {
        OnMessageDeleted?.Invoke(msg, user);
        _db.DeleteMessage(msg.MessageId, user.UserId);
    }

    public Chat CreateChat(Chat chat)
    {
        Chat newChat = _db.CreateChat(chat.ChatName, chat.OwnerId, chat.Type);
        OnChatCreated?.Invoke(newChat, _db.GetUser(newChat.OwnerId));
        return newChat;
    }

    public Chat CreatePrivateChat(int user1, int user2)
    {
        Chat chat = _db.CreatePrivateChat(user1, user2);
        OnPrivateCreated?.Invoke(chat, _db.GetUser(user1), _db.GetUser(user2));
        return chat;
    }

    public Chat JoinChat(Chat chat, User user)
    {
        Chat newChat = _db.JoinChat(chat.ChatId, user.UserId);
        OnJoinToChat?.Invoke(newChat, user);
        return newChat;
    }

    public void AddUserToGroup(Chat chat, int owner, int user)
    {
        OnAddToChat?.Invoke(chat, _db.GetUser(owner), _db.GetUser(user));
        _db.AddUserToGroup(chat.ChatId, owner, user);
    }

    public void LeaveChat(Chat chat, User user) => _db.LeaveChat(chat.ChatId, user.UserId);
    public void ClearChat(Chat chat, User user) => _db.ClearChat(chat.ChatId, user.UserId);
    public void DeleteChat(Chat chat, User user) => _db.DeleteChat(chat.ChatId, user.UserId);
    public Chat GetChat(int chatId) => _db.GetChat(chatId);

    public List<User> GetChatMembers(Chat chat) => _db.GetChatMembers(chat.ChatId);
    public List<Message> GetMessages(Chat chat) => _db.GetMessages(chat.ChatId);

    public List<Chat> GetChats(int user) => _db.GetChatsOfUser(user);
}