using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _messageRepository = messageRepository;
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var currentUsername = Context.User.GetUsername();
            if (currentUsername == createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send message to yourself!");
            }

            // 1. transfer CreateMessageDto to Message
            var currentUser = await _userRepository.GetUserByUsernameAsync(currentUsername);
            var recipientUser = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());

            if (recipientUser == null)
            {
                throw new HubException("Not found user");
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
                var groupName = GetGroupName(currentUser.UserName, recipientUser.UserName);
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }

        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var recipient = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), recipient);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), recipient);
            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string callerName, string recipientName)
        {
            var groupName =
                string.CompareOrdinal(callerName, recipientName) < 0 ?
                callerName + "-" + recipientName :
                recipientName + "-" + callerName;
            return groupName;
        }
    }
}