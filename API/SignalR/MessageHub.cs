using System;
using System.Linq;
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
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;
        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {
            _tracker = tracker;
            _presenceHub = presenceHub;
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

            #region if recipient is connected to current group in hub, mark as read
            var groupName = GetGroupName(currentUser.UserName, recipientUser.UserName);
            var group = await _messageRepository.GetGroupAsync(groupName);

            if (group.Connections.Any(conn => conn.Username == recipientUser.UserName))
            {
                message.DateTimeRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _tracker.GetConnectionsForUser(recipientUser.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections)
                        .SendAsync(
                            "NewMessageReceived",
                            new 
                            {
                                username = currentUser.UserName,
                                knownAs = currentUser.KnownAs
                            }
                        );
                }
            }

            #endregion


            // 2. Save transferred Message to MessageRepo
            _messageRepository.AddMessage(message);

            // 3. Return created MessageDto
            if (await _messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }

        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var recipient = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), recipient);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddConnectionToGroupAsync(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), recipient);
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var group = await RemoveConnectionFromGroupAsync();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

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

        private async Task<Group> AddConnectionToGroupAsync(string groupName)
        {
            var group = await _messageRepository.GetGroupAsync(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if(await _messageRepository.SaveAllAsync())
            {
                return group;
            }

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveConnectionFromGroupAsync()
        {
            var group = await _messageRepository.GetGroupForConnectionAsync(Context.ConnectionId);
            var connection = group.Connections.SingleOrDefault(conn => conn.ConnectionId == Context.ConnectionId);

            _messageRepository.RemoveConnection(connection);

            if (await _messageRepository.SaveAllAsync())
            {
                return group;
            }

            throw new HubException("Failed to remove connection from group");
        }

    }
}