using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

	public User RegisterUser(string username, string password)
	{
		var command = new NpgsqlCommand(
			@"
			INSERT INTO users (username, password)
			SELECT @username, @password
			WHERE NOT EXISTS (
				SELECT 1 FROM users WHERE username = @username
			)
			RETURNING userid;",
			connection
		);

		command.Parameters.AddWithValue("@username", username);
		command.Parameters.AddWithValue("@password", Sypher.Encode(password)); 

		int newUserId = (int)(command.ExecuteScalar() ?? -1);
		command.Dispose();

		User newUser = new User {
			UserId = newUserId,
			UserName = username,
			Password = password
		};

		return newUser;
	}

	public User LoginUser(string username, string password)
	{
		var command = new NpgsqlCommand(
			"SELECT userid FROM users WHERE username=@username AND password=@password",
			connection
		);
		command.Parameters.AddWithValue("@username", username);
		command.Parameters.AddWithValue("@password", Sypher.Encode(password));

		int userId = (int)(command.ExecuteScalar() ?? -1);
		command.Dispose();



		User user = new User {
			UserId = userId,
			UserName = username,
			Password = password
		};

		return user;
	}

	public User? GetUser(int userId)
	{
		using var command = new NpgsqlCommand(@"
			SELECT userid, username
			FROM users
			WHERE userid = @id
		", connection);

		command.Parameters.AddWithValue("@id", userId);

		using var reader = command.ExecuteReader();

		if (!reader.Read())
			return null;

		return new User
		{
			UserId = reader.GetInt32(0),    
			UserName = reader.GetString(1)
		};
	}


	public int SendMessage(int chatid, int userid, string text, int? seconds = null)
	{
		var chatCheck = new NpgsqlCommand(
			"SELECT type, ownerid FROM chats WHERE chatid=@chatid",
			connection
		);
		chatCheck.Parameters.AddWithValue("@chatid", chatid);

		int chatType;
		int ownerId;

		using (var reader = chatCheck.ExecuteReader())
		{
			if (!reader.Read())
				throw new InvalidOperationException("Чат не найден.");

			chatType = reader.GetInt32(0);
			ownerId = reader.GetInt32(1);
		}

		if (chatType == 2 && userid != ownerId)
			throw new InvalidOperationException("Только владелец может писать в этом канале.");

		if (chatType != 2)
		{
			var checkCommand = new NpgsqlCommand(
				"SELECT 1 FROM chatmembers WHERE chatid=@chatid AND userid=@userid",
				connection
			);
			checkCommand.Parameters.AddWithValue("@chatid", chatid);
			checkCommand.Parameters.AddWithValue("@userid", userid);

			if (checkCommand.ExecuteScalar() == null)
				throw new InvalidOperationException("Пользователь не состоит в этом чате.");
		}

    var insertCmd = new NpgsqlCommand(@"
        INSERT INTO messages (chatid, userid, messagetext, createdat)
        VALUES (@chatid, @userid, @text, NOW())
        RETURNING messageid;
    ", connection);

    insertCmd.Parameters.AddWithValue("@chatid", chatid);
    insertCmd.Parameters.AddWithValue("@userid", userid);
    insertCmd.Parameters.AddWithValue("@text", text);

    int newMessageId = (int)insertCmd.ExecuteScalar();

    if (seconds.HasValue && seconds.Value > 0)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(seconds.Value * 1000);

            using var conn = new NpgsqlConnection(connection.ConnectionString);
            conn.Open();

            var updateCmd = new NpgsqlCommand(
                "UPDATE messages SET messagetext='Message Expired' WHERE messageid=@msgid",
                conn
            );
            updateCmd.Parameters.AddWithValue("@msgid", newMessageId);

            updateCmd.ExecuteNonQuery();
        });
    }

    return newMessageId;
}




	public Chat? GetChat(int chatId)
	{
		using var command = new NpgsqlCommand(
			@"SELECT chatid, chatname, type, ownerid
			FROM chats 
			WHERE chatid = @id",
			connection
		);
		command.Parameters.AddWithValue("@id", chatId);

		using var reader = command.ExecuteReader();
		if (!reader.Read())
			return null;

		return new Chat
		{
			ChatId = reader.GetInt32(reader.GetOrdinal("chatid")),
			ChatName = reader.GetString(reader.GetOrdinal("chatname")),
			Type = reader.GetInt32(reader.GetOrdinal("type")),
			OwnerId = reader.IsDBNull(reader.GetOrdinal("ownerid")) ? -1 : reader.GetInt32(reader.GetOrdinal("ownerid"))
		};
	}


	public Chat CreatePrivateChat(int user1Id, int user2Id)
	{
		// Проверка, существует ли уже чат между этими двумя
		var checkCommand = new NpgsqlCommand(
			@"SELECT c.chatid
			FROM chats c
			JOIN chatmembers m1 ON c.chatid = m1.chatid AND m1.userid=@user1
			JOIN chatmembers m2 ON c.chatid = m2.chatid AND m2.userid=@user2
			WHERE c.type=0",
			connection
		);
		checkCommand.Parameters.AddWithValue("@user1", user1Id);
		checkCommand.Parameters.AddWithValue("@user2", user2Id);

		object existingChat = checkCommand.ExecuteScalar();
		checkCommand.Dispose();

		if (existingChat != null)
			return (Chat)existingChat;

		// Создаём новый чат
		var createCommand = new NpgsqlCommand(
			@"INSERT INTO chats (chatname, type) 
			VALUES (@chatname, 0) 
			RETURNING chatid",
			connection
		);
		createCommand.Parameters.AddWithValue("@chatname", $"Личный чат {user1Id}-{user2Id}");
		int chatId = (int)(createCommand.ExecuteScalar() ?? -1);
		createCommand.Dispose();

		// Добавляем обоих пользователей
		var addMembers = new NpgsqlCommand(
			"INSERT INTO chatmembers (chatid, userid) VALUES (@chatid, @userid1), (@chatid, @userid2)",
			connection
		);
		addMembers.Parameters.AddWithValue("@chatid", chatId);
		addMembers.Parameters.AddWithValue("@userid1", user1Id);
		addMembers.Parameters.AddWithValue("@userid2", user2Id);
		addMembers.ExecuteNonQuery();
		addMembers.Dispose();

		return GetChat(chatId);
	}


	public Chat CreateChat(string chatname, int ownerId, int type)
	{
		var command = new NpgsqlCommand(
			@"INSERT INTO chats (chatname, ownerid, type) 
			VALUES (@chatname, @ownerid, @type) 
			RETURNING chatid",
			connection
		);
		command.Parameters.AddWithValue("@chatname", chatname);
		command.Parameters.AddWithValue("@ownerid", ownerId);
		command.Parameters.AddWithValue("@type", type);

		int chatId = (int)(command.ExecuteScalar() ?? -1);
		command.Dispose();

		command = new NpgsqlCommand(
			"INSERT INTO chatmembers (chatid, userid) VALUES (@chatid, @userid) ON CONFLICT DO NOTHING",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatId);
		command.Parameters.AddWithValue("@userid", ownerId);
		command.ExecuteNonQuery();
		command.Dispose();

		return GetChat(chatId);
	}

	public Chat JoinChat(int chatid, int userid)
	{
		var chatCommand = new NpgsqlCommand(
			"SELECT type FROM chats WHERE chatid=@chatid",
			connection
		);
		chatCommand.Parameters.AddWithValue("@chatid", chatid);

		object result = chatCommand.ExecuteScalar();
		chatCommand.Dispose();

		if (result == null)
			throw new InvalidOperationException("Чат не найден.");

		int type  = Convert.ToInt32(result);
;

		if (type != 2)
			throw new InvalidOperationException("Присоединяться можно только к каналам.");

		var command = new NpgsqlCommand(
			"INSERT INTO chatmembers (chatid, userid) VALUES (@chatid, @userid) ON CONFLICT DO NOTHING",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);
		command.Parameters.AddWithValue("@userid", userid);
		command.ExecuteNonQuery();
		command.Dispose();

		return GetChat(chatid);
	}


	public void AddUserToGroup(int chatid, int ownerId, int newUserId)
	{
		var chatCommand = new NpgsqlCommand(
			"SELECT type, ownerid FROM chats WHERE chatid=@chatid",
			connection
		);
		chatCommand.Parameters.AddWithValue("@chatid", chatid);

		int type, actualOwner;
		using (var reader = chatCommand.ExecuteReader())
		{
			if (!reader.Read()) throw new InvalidOperationException("Чат не найден.");
			type = reader.GetInt32(0);
			actualOwner = reader.GetInt32(1);
		}
		chatCommand.Dispose();

		if (type != 1) 
			throw new InvalidOperationException("Можно добавлять пользователей только в группы.");

		if (ownerId != actualOwner) 
			throw new InvalidOperationException("Добавлять пользователей может только владелец группы.");

		var command = new NpgsqlCommand(
			"INSERT INTO chatmembers (chatid, userid) VALUES (@chatid, @userid) ON CONFLICT DO NOTHING",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);
		command.Parameters.AddWithValue("@userid", newUserId);
		command.ExecuteNonQuery();
		command.Dispose();
	}


	public void LeaveChat(int chatid, int userid)
	{
		var command = new NpgsqlCommand(
			"DELETE FROM chatmembers WHERE chatid=@chatid AND userid=@userid",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);
		command.Parameters.AddWithValue("@userid", userid);
		command.ExecuteNonQuery();
		command.Dispose();
	}

	public void ClearChat(int chatid, int userid)
	{
		var chatCommand = new NpgsqlCommand("SELECT type, ownerid FROM chats WHERE chatid=@chatid", connection);
		chatCommand.Parameters.AddWithValue("@chatid", chatid);

		int type, ownerId;
		using (var reader = chatCommand.ExecuteReader())
		{
			if (!reader.Read()) throw new InvalidOperationException("Чат не найден.");
			type = reader.GetInt32(0);
			ownerId = reader.GetInt32(1);
		}
		chatCommand.Dispose();

		// Проверка прав
		if (type == 0)
		{
			var memberCheck = new NpgsqlCommand("SELECT 1 FROM chatmembers WHERE chatid=@chatid AND userid=@userid", connection);
			memberCheck.Parameters.AddWithValue("@chatid", chatid);
			memberCheck.Parameters.AddWithValue("@userid", userid);
			if (memberCheck.ExecuteScalar() == null && userid != ownerId)
				throw new InvalidOperationException("Нет прав очистить этот чат.");
			memberCheck.Dispose();
		}
		else
		{
			if (userid != ownerId)
				throw new InvalidOperationException("Нет прав очистить этот чат.");
		}

		var deleteCommand = new NpgsqlCommand("DELETE FROM messages WHERE chatid=@chatid", connection);
		deleteCommand.Parameters.AddWithValue("@chatid", chatid);
		deleteCommand.ExecuteNonQuery();
		deleteCommand.Dispose();
	}

	public void DeleteMessage(int messageId, int userid)
	{
		var msgCommand = new NpgsqlCommand("SELECT chatid, userid FROM messages WHERE messageid=@msgid", connection);
		msgCommand.Parameters.AddWithValue("@msgid", messageId);
		int chatId, msgOwnerId;
		using (var reader = msgCommand.ExecuteReader())
		{
			if (!reader.Read()) throw new InvalidOperationException("Сообщение не найдено.");
			chatId = reader.GetInt32(0);
			msgOwnerId = reader.GetInt32(1);
		}
		msgCommand.Dispose();

		var chatCommand = new NpgsqlCommand("SELECT type, ownerid FROM chats WHERE chatid=@chatid", connection);
		chatCommand.Parameters.AddWithValue("@chatid", chatId);
		int type, ownerId;
		using (var reader = chatCommand.ExecuteReader())
		{
			if (!reader.Read()) throw new InvalidOperationException("Чат не найден.");
			type = reader.GetInt32(0);
			ownerId = reader.GetInt32(1);
		}
		chatCommand.Dispose();

		bool canDelete = type switch
		{
			0 => msgOwnerId == userid, // личка — только владелец сообщения
			1 => userid == ownerId || userid == msgOwnerId, // группа — владелец чата или сообщения
			2 => userid == ownerId, // канал — только владелец
			_ => false
		};

		if (!canDelete)
			throw new InvalidOperationException("Нет прав удалить это сообщение.");

		var deleteCommand = new NpgsqlCommand("DELETE FROM messages WHERE messageid=@msgid", connection);
		deleteCommand.Parameters.AddWithValue("@msgid", messageId);
		deleteCommand.ExecuteNonQuery();
		deleteCommand.Dispose();
	}


	public List<User> GetChatMembers(int chatid)
	{
		var members = new List<User>();

		var command = new NpgsqlCommand(
			@"SELECT u.userid, u.username 
			FROM users u
			JOIN chatmembers cm ON u.userid = cm.userid
			WHERE cm.chatid=@chatid",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);

		using (var reader = command.ExecuteReader())
		{
			while (reader.Read())
			{
				members.Add(new User
				{
					UserId = reader.GetInt32(0),
					UserName = reader.GetString(1)
				});
			}
		}
		command.Dispose();
		return members;
	}

	public List<Chat> GetChatsOfUser(int userId)
	{
		var chats = new List<Chat>();

		var command = new NpgsqlCommand(
			@"SELECT c.chatid, c.chatname
			FROM chats c
			JOIN chatmembers cm ON c.chatid = cm.chatid
			WHERE cm.userid = @userid",
			connection
		);

		command.Parameters.AddWithValue("@userid", userId);

		using (var reader = command.ExecuteReader())
		{
			while (reader.Read())
			{
				chats.Add(new Chat
				{
					ChatId = reader.GetInt32(0),
					ChatName = reader.GetString(1),
				});
			}
		}
		command.Dispose();
		return chats;
	}

	public List<Message> GetMessages(int chatid)
	{
		var messages = new List<Message>();

		var command = new NpgsqlCommand(
			@"SELECT 
				m.messageid,
				m.chatid,
				m.userid,
				m.messagetext,
				m.timestamp,
				u.username
			FROM messages m
			JOIN users u ON u.userid = m.userid
			WHERE m.chatid = @chatid
			ORDER BY m.timestamp ASC;
			",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);

		using (var reader = command.ExecuteReader())
		{
			while (reader.Read())
			{
				messages.Add(new Message
				{
					MessageId = reader.GetInt32(0),
					ChatId = reader.GetInt32(1),
					User = new User
					{
						UserId = reader.GetInt32(2),
						UserName = reader.GetString(5)
					},
					Text = reader.GetString(3),
					CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
				});
			}
		}
		command.Dispose();
		return messages;
	}

	public void DeleteChat(int chatid, int userId)
	{
		var chat = GetChat(chatid);
		if (chat == null)
			throw new InvalidOperationException("Чат не найден.");

		// Проверка прав: владелец для группы/канала, любой для лички
		if (chat.Type != 0 && chat.OwnerId != userId)
			throw new InvalidOperationException("Нет прав удалить этот чат.");

		using var command = new NpgsqlCommand(
			"DELETE FROM chats WHERE chatid=@chatid",
			connection
		);
		command.Parameters.AddWithValue("@chatid", chatid);
		command.ExecuteNonQuery();
	}

}