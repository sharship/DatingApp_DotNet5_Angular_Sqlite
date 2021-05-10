using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public async Task<Group> GetGroupAsync(string groupName)
        {
            return await _context.Groups
                .Include(g => g.Connections)
                .SingleOrDefaultAsync(g => g.Name == groupName);
        }

        public async Task<Group> GetGroupForConnectionAsync(string connectionId)
        {
            return await _context.Groups
                .Include(g => g.Connections)
                .SingleOrDefaultAsync(g =>
                    g.Connections.Any(conn => conn.ConnectionId == connectionId)
                );
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<Connection> GetConnectionAsync(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }



        // Create basic
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        // Delete basic
        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }



        // Read basic
        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        // Read specific messages, to support fuctionality
        public async Task<PagedList<MessageDto>> GetMessagesForCurrentUser(MessageParams messageParams)
        {
            // get IQueryable<Message>
            var messageQuery = _context.Messages
                .OrderByDescending(m => m.DateTimeSent)
                .AsQueryable();

            switch (messageParams.Container)
            {
                case "Inbox":
                    messageQuery = messageQuery.Where(m =>
                        m.RecipientUsername == messageParams.CurrentUsername
                        && m.RecipientDeleted == false
                    );
                    break;

                case "Outbox":
                    messageQuery = messageQuery.Where(m =>
                        m.SenderUsername == messageParams.CurrentUsername
                        && m.SenderDeleted == false
                    );
                    break;

                default:
                    // Unread
                    messageQuery = messageQuery.Where(m =>
                        m.RecipientUsername == messageParams.CurrentUsername
                        && m.DateTimeRead == null
                        && m.RecipientDeleted == false
                    );
                    break;
            };

            // execute IQueryable<Message>, and then project to IQueryable<MessageDto>
            var messageDtos = messageQuery.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            // create paged list
            return await PagedList<MessageDto>.CreateAsync(messageDtos, messageParams.PageNumber, messageParams.PageSize);

        }

        // Read message thread, to support fuctionality
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            // 1. get messages from both sides of conversation
            var messages = await _context.Messages
                .Include(m => m.Sender).ThenInclude(s => s.Photos)
                .Include(m => m.Recipient).ThenInclude(r => r.Photos)
                .Where(m =>
                   // current use is sender
                   m.Sender.UserName == currentUsername && m.Recipient.UserName == recipientUsername
                   && m.SenderDeleted == false
                   ||
                   // current user is recipient
                   m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername
                   && m.RecipientDeleted == false
                )
                .OrderBy(m => m.DateTimeSent)
                .ToListAsync();

            // 2. mark Unread to Read, and save back to DB
            var unreadMessages = messages
                .Where(m =>
                    m.Recipient.UserName == currentUsername && m.DateTimeRead == null
                )
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.DateTimeRead = DateTime.UtcNow;
                }
            }

            // 3. project and return all conversation messages
            return _mapper.Map<IEnumerable<MessageDto>>(messages);

        }



    }
}