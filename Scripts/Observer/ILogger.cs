public interface ILogger 
{

    void SubscribeToEvents(ChatService chatService);

    void LogUserRegistered(User user);
    void LogUserLogin(User user);
    void LogChatCreated(Chat chat, User user1 = null, User user2 = null);
    void LogPrivateChatCreated(Chat chat, User user1 = null, User user2 = null);
    void LogMessageSent(Message message, User user = null, int? seconds = null);
    void LogMessageDeleted(Message message, User user = null, int? seconds = null);
    void LogExpiringMessageSent(Message message, User user = null, int? seconds = null);
    void LogExpiringMessageDeleted(Message message, User user = null, int? seconds = null);
    void LogJoinToChat(Chat chat, User user = null, User newUser = null);
    void LogAddToChat(Chat chat, User user1 = null, User user2 = null);

	void LogInformation(string text);
}