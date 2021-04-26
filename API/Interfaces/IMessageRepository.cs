using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        // Basic CRUD methods
        // Create
        void AddMessage(Message message);

        // Delete
        void DeleteMessage(Message message);

        // Read
        Task<Message> GetMessage(int id);

        // Update
        // ...

        // Funcitonal metods
        Task<PagedList<MessageDto>> GetMessagesForCurrentUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername);

        // Save to DB
        Task<bool> SaveAllAsync();
    }
}