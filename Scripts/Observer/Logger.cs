using System.Diagnostics.Eventing.Reader;

public class Logger : ILogger
{
    private readonly ChatService chatService;

    public Logger(ChatService chatService)
    {
        this.chatService = chatService;
        SubscribeToEvents(chatService);
    }

    public void SubscribeToEvents(ChatService chatService)
    {
        chatService.OnUserRegistred += LogUserRegistered;
        chatService.OnUserLogined += LogUserLogin;

        chatService.OnChatCreated += LogChatCreated;
        chatService.OnPrivateCreated += LogPrivateChatCreated;

        chatService.OnMessageSended += LogMessageSent;
        chatService.OnMessageDeleted += LogMessageDeleted;
        chatService.OnExpiringMessageSended += LogExpiringMessageSent;
        chatService.OnExpiringMessageDeleted += LogExpiringMessageDeleted;

        chatService.OnJoinToChat += LogJoinToChat;
        chatService.OnAddToChat += LogAddToChat;
    }

    public void LogUserRegistered(User user)
    {
        LogInformation($"User registered: ID:{user.UserId}, Username:{user.UserName}");
    }

    public void LogUserLogin(User user)
    {
        LogInformation($"User logged in: ID={user.UserId}, Username={user.UserName}");
    }

    public void LogChatCreated(Chat chat, User user1, User user2)
    {
        LogInformation($"Chat created: ID={chat.ChatId}, Name={chat.ChatName}, Type={chat.Type}, OwnerID={chat.OwnerId}");
    }

    public void LogPrivateChatCreated(Chat chat, User user1, User user2)
    {
        LogInformation($"Private chat created: ID={chat.ChatId}, Between User1={user1?.UserId} and User2={user2?.UserId}");
    }

    public void LogMessageSent(Message message, User user, int? seconds)
    {
        LogInformation($"Message sent: ID={message.MessageId}, ChatID={message.ChatId}, UserID={message.User?.UserId}, Text={message.Text}");
    }

    public void LogMessageDeleted(Message message, User user, int? seconds)
    {
        LogInformation($"Message deleted: ID={message.MessageId}, ChatID={message.ChatId}, By UserID={user?.UserId}");
    }

    public void LogExpiringMessageSent(Message message, User user, int? seconds)
    {
        LogInformation($"Expiring message sent: ID={message.MessageId}, ChatID={message.ChatId}, UserID={message.User?.UserId}, Expires in {seconds} seconds");
    }

    public void LogExpiringMessageDeleted(Message message, User user, int? seconds)
    {
        LogInformation($"Expiring message auto-deleted: ID={message.MessageId}, ChatID={message.ChatId}");
    }

    public void LogJoinToChat(Chat chat, User user, User newUser)
    {
        LogInformation($"User joined chat: UserID={user?.UserId}, ChatID={chat.ChatId}");
    }

    public void LogAddToChat(Chat chat, User user1, User user2)
    {
        LogInformation($"User added to chat: AddedUserID={user2?.UserId}, ChatID={chat.ChatId}, By UserID={user1?.UserId}");
    }

    public void LogInformation(string text)
    {
        Console.WriteLine(text);
    }
}