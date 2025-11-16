using System.Data.SqlTypes;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService chatService;

    public ChatController(ChatService chatService)
    {
        this.chatService = chatService;
    }

    // ----------------- Пользователи -----------------
    [HttpPost("register")]
    public IActionResult RegisterUser([FromBody] User user)
    {
        User newUser = chatService.RegisterUser(user);
        return Ok(newUser);
    }

    [HttpPost("login")]
    public IActionResult LoginUser([FromBody] User user)
    {
        User newUser = chatService.LoginUser(user);
        return Ok(newUser);
    }

    // ----------------- Чаты -----------------
    [HttpPost("createprivate/{user2Id}")]
    public IActionResult CreatePrivateChat([FromBody] Chat chat , int user2Id)
    {
        var chatId = chatService.CreatePrivateChat(chat.OwnerId, user2Id);
        return Ok(chatId);
    }

    [HttpPost("create")]
    public IActionResult CreateChat([FromBody] Chat chat)
    {
        Chat newChat = chatService.CreateChat(chat);
        return Ok(newChat);
    }

    [HttpPost("{chatId}/join")]
    public IActionResult JoinChat(int chatId, [FromBody] User user)
    {
        var chat = new Chat { ChatId = chatId };
        return Ok(chatService.JoinChat(chat, user));
    }

    [HttpPost("{chatId}/addUser")]
    public IActionResult AddUserToGroup(int chatId, [FromBody] AddUserRequest req)
    {
        var chat = new Chat { ChatId = chatId };
        chatService.AddUserToGroup(chat, req.Owner, req.NewUser);
        return Ok();
    }

    [HttpPost("{chatId}/leave")]
    public IActionResult LeaveChat(int chatId, [FromBody] User user)
    {
        var chat = new Chat { ChatId = chatId };
        chatService.LeaveChat(chat, user);
        return Ok();
    }

    [HttpPost("{chatId}/clear")]
    public IActionResult ClearChat(int chatId, [FromBody] User user)
    {
        var chat = new Chat { ChatId = chatId };
        chatService.ClearChat(chat, user);
        return Ok();
    }

    [HttpDelete("{chatId}/deleteChat")]
    public IActionResult DeleteChat(int chatId, [FromBody] User user)
    {
        var chat = new Chat { ChatId = chatId };
        chatService.DeleteChat(chat, user);
        return Ok();
    }

    [HttpGet("{chatId}/getChat")]
    public IActionResult GetChat (int chatId)
    {
        return Ok(chatService.GetChat(chatId));
    }



    [HttpPost("{chatId}/send")]
    public IActionResult SendMessage(int chatId, [FromBody] Message msg)
    {
        msg.ChatId = chatId; 
        if(msg.ExpiringTime == 0)
		{
			chatService.SendMessage(msg);
		}else
		{
			chatService.SendExpiringMessage(msg , msg.ExpiringTime);
		}
        return Ok("sent");
    }


    [HttpDelete("{chatId}/deleteMsg/{messageId}")]
    public IActionResult DeleteMessage(int chatId, int messageId, [FromBody] User user)
    {
        var msg = new Message { MessageId = messageId, ChatId = chatId };
        chatService.DeleteMessage(msg, user);
        return Ok();
    }

    [HttpGet("{chatId}/members")]
    public IActionResult GetChatMembers(int chatId)
    {
        var chat = new Chat { ChatId = chatId };
        var members = chatService.GetChatMembers(chat);
        return Ok(members);
    }

    [HttpGet("{chatId}/messages")]
    public IActionResult GetMessages(int chatId)
    {
        var chat = new Chat { ChatId = chatId };
        var messages = chatService.GetMessages(chat);
        return Ok(messages);
    }

    [HttpGet("{userId}/chats")]
    public IActionResult GetChats(int userId)
    {
        var chats = chatService.GetChats(userId);
        return Ok(chats);
    }
}

