public interface IDatabaseManager 
{
	public void SendMessage(int chatid, int userid, string message);
	public int RegisterUser(string username , string password);
	public int LoginUser(string username , string password);
	public int CreateChat(string chatname);
	public int JoinChat(string chatname);
	public void AddUserToChat(int chatid , int userid);
}
