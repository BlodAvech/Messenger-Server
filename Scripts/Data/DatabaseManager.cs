using Npgsql;
using System;
using System.Collections.Generic;

public class DatabaseManager : IDatabaseManager
{
	const string connectionString = "Host=localhost;Username=postgres;Password=1502;Database=postgres";
	private NpgsqlConnection? connection;

	public DatabaseManager()
	{
		connection = new NpgsqlConnection(connectionString);
		connection.Open();
	}

	public void Close()
	{
		if (connection != null)
		{
			connection.Close();
			connection.Dispose();
			connection = null;
		}
	}

	public int RegisterUser(string username, string password)
	{
		var command = new NpgsqlCommand(
			@"INSERT INTO users (username, password) 
			VALUES (@username, @password) 
			RETURNING userid",
			connection
		);
		command.Parameters.AddWithValue("@username", username);
		command.Parameters.AddWithValue("@password", Sypher.Encode(password)); 

		int newUserId = (int)(command.ExecuteScalar() ?? -1);
		command.Dispose();
		return newUserId;
	}

	public int LoginUser(string username, string password)
	{
		var command = new NpgsqlCommand(
			"SELECT userid FROM users WHERE username=@username AND password=@password",
			connection
		);
		command.Parameters.AddWithValue("@username", username);
		command.Parameters.AddWithValue("@password", Sypher.Encode(password));

		object result = command.ExecuteScalar();
		command.Dispose();

		if (result != null)
			return (int)result;

		return -1; 
	}

	public int CreateChat(string chatname)
	{
		var command = new NpgsqlCommand(
			@"INSERT INTO chats (chatname) 
			VALUES (@chatname) 
			RETURNING chatid",
			connection
		);
		command.Parameters.AddWithValue("@chatname", chatname);

		int chatId = (int)(command.ExecuteScalar() ?? -1);
		command.Dispose();
		return chatId;
	}

	public void AddUserToChat(int chatid, int userid)
	{
		var command = new NpgsqlCommand(
			"INSERT INTO chatmembers (chatid, userid) VALUES (@chatid, @userid)",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);
		command.Parameters.AddWithValue("@userid", userid);

		command.ExecuteNonQuery();
		command.Dispose();
	}

	public void SendMessage(int chatid, int userid, string message)
	{
		var checkCommand = new NpgsqlCommand(
		"SELECT 1 FROM chatmembers WHERE chatid=@chatid AND userid=@userid",
		connection
		);
		checkCommand.Parameters.AddWithValue("@chatid", chatid);
		checkCommand.Parameters.AddWithValue("@userid", userid);

		object exists = checkCommand.ExecuteScalar();
		checkCommand.Dispose();

		if (exists == null)
			throw new InvalidOperationException("Пользователь не состоит в этом чате.");
		
		var command = new NpgsqlCommand(
			@"INSERT INTO messages (chatid, userid, messagetext) 
			VALUES (@chatid, @userid, @message)",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);
		command.Parameters.AddWithValue("@userid", userid);
		command.Parameters.AddWithValue("@message", message);

		command.ExecuteNonQuery();
		command.Dispose();
	}

	public int JoinChat(string chatname)
	{
		throw new NotImplementedException();
	}
}