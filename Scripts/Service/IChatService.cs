public interface IChatService
{
    event UserEventHandler OnUserRegistred;
    event UserEventHandler OnUserLogined;
    event ChatEventHandler OnChatCreated;
    event ChatEventHandler OnPrivateCreated;
    event MessageEventHandler OnMessageSended;
    event MessageEventHandler OnMessageDeleted;
    event MessageEventHandler OnExpiringMessageSended;
    event MessageEventHandler OnExpiringMessageDeleted;
    event ChatEventHandler OnJoinToChat;
    event ChatEventHandler OnAddToChat;

    User LoginUser(User user);
    User RegisterUser(User user);

    void SendMessage(Message msg);
    Task SendExpiringMessage(Message msg, int seconds);
    void DeleteMessage(Message msg, User user);

    Chat CreateChat(Chat chat);
    Chat CreatePrivateChat(int user1, int user2);
    Chat JoinChat(Chat chat, User user);
    void AddUserToGroup(Chat chat, int owner, int user);
    void LeaveChat(Chat chat, User user);
    void ClearChat(Chat chat, User user);
    void DeleteChat(Chat chat, User user);

    Chat GetChat(int chatId);
    List<User> GetChatMembers(Chat chat);
    List<Message> GetMessages(Chat chat);
    List<Chat> GetChats(int user);
}

public delegate void UserEventHandler(User user);
public delegate void ChatEventHandler(Chat chat, User? user1 = null, User? user2 = null);
public delegate void MessageEventHandler(Message message, User? user = null, int? seconds = null);