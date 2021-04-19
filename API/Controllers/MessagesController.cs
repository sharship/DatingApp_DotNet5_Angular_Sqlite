using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _mapper = mapper;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        // Create
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var currentUsername = User.GetUsername();
            if (currentUsername == createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot send message to yourself!");
            }

            // 1. transfer CreateMessageDto to Message
            var currentUser = await _userRepository.GetUserByUsernameAsync(currentUsername);
            var recipientUser = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());

            if (recipientUser == null)
            {
                return NotFound();
            }

            var message = new Message
            {   
                SenderUsername = currentUsername,
                Sender = currentUser,

                RecipientUsername = recipientUser.UserName,
                Recipient = recipientUser,

                Content = createMessageDto.Content,
            };

            // 2. Save transferred Message to MessageRepo
            _messageRepository.AddMessage(message);

            // 3. Return created MessageDto
            if (await _messageRepository.SaveAllAsync())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed to send message.");

        }

        // Get
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForCurrentUser([FromQuery] MessageParams messageParams)
        {
            // add value to parameter property
            messageParams.CurrentUsername = User.GetUsername();
            // get messages from Message Repo, based on constructed Params
            var paged_messages = await _messageRepository.GetMessagesForCurrentUser(messageParams);

            // add pagination info to http response header, based on PagedList<MessageDto> from Repo
            Response.AddPaginationHeader(paged_messages.CurrentPage, paged_messages.PageSize, paged_messages.TotalCount, paged_messages.TotalPages);

            return paged_messages;
        }

        // Get Thread
        [HttpGet("thread/{otherusername}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string otherusername)
        {
            var otherUser = await _userRepository.GetUserByUsernameAsync(otherusername);
            if (otherUser == null)
            {
                return NotFound();
            }

            var currentUsername = User.GetUsername();
            
            var messages = await _messageRepository.GetMessageThread(currentUsername, otherusername);

            return Ok(messages);
        }

    }
}