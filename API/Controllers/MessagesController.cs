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
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            var currentUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(currentUsername);
            var recipientUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());

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
            _unitOfWork.MessageRepository.AddMessage(message);

            // 3. Return created MessageDto
            if (await _unitOfWork.Complete())
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
            var paged_messages = await _unitOfWork.MessageRepository.GetMessagesForCurrentUser(messageParams);

            // add pagination info to http response header, based on PagedList<MessageDto> from Repo
            Response.AddPaginationHeader(paged_messages.CurrentPage, paged_messages.PageSize, paged_messages.TotalCount, paged_messages.TotalPages);

            return paged_messages;
        }

        // Get Thread
        [HttpGet("thread/{otherusername}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string otherusername)
        {
            var otherUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(otherusername);
            if (otherUser == null)
            {
                return NotFound();
            }

            var currentUsername = User.GetUsername();

            var messages = await _unitOfWork.MessageRepository.GetMessageThread(currentUsername, otherusername);

            return Ok(messages);
        }

        // Delete
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var message = await _unitOfWork.MessageRepository.GetMessage(id);
            var currentUsername = User.GetUsername();

            if (message.Sender.UserName != currentUsername && message.Recipient.UserName != currentUsername)
            {
                return Unauthorized();
            }

            // sender delete
            if (message.Sender.UserName == currentUsername)
            {
                message.SenderDeleted = true;
            }

            // recipient delete
            if (message.Recipient.UserName == currentUsername)
            {
                message.RecipientDeleted = true;
            }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if (await _unitOfWork.Complete())
            {
                return Ok();
            }

            return BadRequest("Fail to delete message.");


        }

    }
}