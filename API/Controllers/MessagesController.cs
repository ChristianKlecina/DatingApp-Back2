
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMessageRepository messagesRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public MessagesController(IMessageRepository messagesRepository, IUserRepository userRepository, IMapper mapper)
        {
            this.messagesRepository = messagesRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }


        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot send message to yourself");
            }

            var sender = await userRepository.GetUserByUsernameAsync(username);
            var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if(recipient == null)
            {
                return NotFound();
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.Username,
                RecipientUsername = recipient.Username,
                Content = createMessageDto.Content
            };

            messagesRepository.AddMessage(message);

            if (await messagesRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await messagesRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return Ok(messages);
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await messagesRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await messagesRepository.GetMessage(id);

            if (message.Sender.Username != username && message.Recipient.Username != username) return Unauthorized();

            if (message.Sender.Username == username) message.SenderDeleted = true;

            if (message.Recipient.Username == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted) messagesRepository.DeleteMessage(message);

            if (await messagesRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
