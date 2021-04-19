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

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForCurrentUser(MessageParams messageParams)
        {
            // get IQueryable<Message>
            var messageQuery = _context.Messages
                .OrderByDescending(m => m.DateTimeSent)
                .AsQueryable();

            switch (messageParams.Container)
            {
                case "Inbox":
                    messageQuery = messageQuery.Where(m => m.RecipientUsername == messageParams.CurrentUsername);
                    break;
                
                case "Outbox":
                    messageQuery = messageQuery.Where(m => m.SenderUsername == messageParams.CurrentUsername);
                    break;

                default:
                // Unread
                    messageQuery = messageQuery.Where(m => m.RecipientUsername == messageParams.CurrentUsername && m.DateTimeRead == null);
                    break;
            };

            // execute IQueryable<Message>, and then project to MessageDto
            var messageDtos = messageQuery.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            // create paged list
            return await PagedList<MessageDto>.CreateAsync(messageDtos, messageParams.PageNumber, messageParams.PageSize);
            
        }

        
        
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            // 1. get messages from both sides of conversation
            var messages = await _context.Messages
                .Include(m => m.Sender).ThenInclude(s => s.Photos)
                .Include(m => m.Recipient).ThenInclude(r => r.Photos)
                .Where( m =>
                    m.Sender.UserName == currentUsername && m.Recipient.UserName == recipientUsername
                    ||
                    m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername
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
                    msg.DateTimeRead = DateTime.Now;
                }

                // save back to DB
                await _context.SaveChangesAsync();
            }

            // 3. project and return all conversation messages
            return _mapper.Map<IEnumerable<MessageDto>>(messages);

        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}