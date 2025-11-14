using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService chatService;
    public ChatController(ChatService chatService) => this.chatService = chatService;

	[HttpPost("send")]
	public IActionResult SendMessage([FromBody] SendMessageDto msgDto)
	{
		chatService.SendMessage(msgDto);
		return Ok("sent");
	}

	[HttpPost("login")]
	public int LoginUser([FromBody] UserDto userDto) => chatService.LoginUser(userDto);

	[HttpPost("register")]
	public int RegisterUser([FromBody] UserDto userDto) => chatService.RegisterUser(userDto);
	
}
