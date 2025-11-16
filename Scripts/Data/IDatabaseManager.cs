public interface IDatabaseManager 
{
// Пользователи
    User RegisterUser(string username, string password);
    User LoginUser(string username, string password);

    // Чаты
    Chat CreatePrivateChat(int user1Id, int user2Id); 
    Chat CreateChat(string chatname, int ownerId, int type); 
    Chat JoinChat(int chatid, int userid); 
    void AddUserToGroup(int chatid, int ownerId, int newUserId); 
    void LeaveChat(int chatid, int userid); 
    void ClearChat(int chatid, int userid); 
    void DeleteChat(int chatid, int ownerId);

    // Сообщения
    int SendMessage(int chatid, int userid, string message);
    void DeleteMessage(int messageId, int userid); 

    // Вспомогательные методы
    List<User> GetChatMembers(int chatid); 
    List<Message> GetMessages(int chatid);

	List<Chat> GetChatsOfUser(int userId);

}
