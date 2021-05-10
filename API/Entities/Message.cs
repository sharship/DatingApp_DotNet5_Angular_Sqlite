using System;

namespace API.Entities
{
    public class Message
    {
        public int Id { get; set; }

        #region Sender
        public int SenderId { get; set; }

        public string SenderUsername { get; set; }

        public AppUser Sender { get; set; }

        #endregion


        #region Recipient
        public int RecipientId { get; set; }
        
        public string RecipientUsername { get; set; }
        
        public AppUser Recipient { get; set; }

        #endregion

        public string Content { get; set; }
        
        public DateTime DateTimeSent { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateTimeRead { get; set; }
        
        public bool SenderDeleted { get; set; }
        
        public bool RecipientDeleted { get; set; }
        
        


    }
}