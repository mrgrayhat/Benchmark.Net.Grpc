using System.ComponentModel.DataAnnotations;

using Benchmark.Net.Grpc.Server.DAL;
using Benchmark.Net6.Grpc.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Benchmark.Net.Grpc.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ChatsController : ControllerBase
    {
        public ILogger<ChatsController> _logger { get; set; }
        public ChatsController(ILogger<ChatsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<MessageContract<List<ChatMessageDto>>> Get()
        {
            List<ChatMessageDto> chats = DbContext.Chats
                                 .Select(chat => new ChatMessageDto()
                                 {
                                     Id = chat.Id,
                                     Message = chat.Message,
                                     User = chat.User
                                 })
                                 .ToList();

            return Ok(new MessageContract<List<ChatMessageDto>>(chats, new PagingResult
            {
                Page = 1,
                PageSize = chats.Count,
                Total = chats.Count
            }));
        }

        [HttpGet("id")]
        public ActionResult<MessageContract<ChatMessageDto>> Get([Required] int id)
        {
            var chat = DbContext.Chats
                .FirstOrDefault(c => c.Id == id);
            if (chat == null)
                return NotFound();

            return Ok(new MessageContract<ChatMessageDto>(new ChatMessageDto
            {
                Id = chat.Id,
                Message = chat.Message,
                User = chat.User
            }));
        }

        [HttpPost]
        public ActionResult<MessageContract<ChatMessageDto>> Add(ChatMessageInfo chat)
        {
            DbContext.Chats.Add(chat);
            return Ok(new MessageContract<ChatMessageDto>(new ChatMessageDto
            {
                Id = chat.Id,
                Message = chat.Message,
                User = chat.User
            }));
        }
    }
}
