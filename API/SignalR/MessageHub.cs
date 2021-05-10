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
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;
        private readonly IUnitOfWork _unitOfWork;
        public MessageHub(IMapper mapper, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _tracker = tracker;
            _presenceHub = presenceHub;
            _mapper = mapper;
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var currentUsername = Context.User.GetUsername();
            if (currentUsername == createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send message to yourself!");
            }

            // 1. transfer CreateMessageDto to Message
            var currentUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(currentUsername);
            var recipientUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());

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
            var group = await _unitOfWork.MessageRepository.GetGroupAsync(groupName);

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
            _unitOfWork.MessageRepository.AddMessage(message);

            // 3. Return created MessageDto
            if (await _unitOfWork.Complete())
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

            var messages = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), recipient);

            if (_unitOfWork.HasChanges())
            {
                await _unitOfWork.Complete();
            }

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
            var group = await _unitOfWork.MessageRepository.GetGroupAsync(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _unitOfWork.Complete())
            {
                return group;
            }

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveConnectionFromGroupAsync()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnectionAsync(Context.ConnectionId);
            var connection = group.Connections.SingleOrDefault(conn => conn.ConnectionId == Context.ConnectionId);

            _unitOfWork.MessageRepository.RemoveConnection(connection);

            if (await _unitOfWork.Complete())
            {
                return group;
            }

            throw new HubException("Failed to remove connection from group");
        }

    }
}