using System;

namespace API.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }

        #region Sender
        public int SenderId { get; set; }

        public string SenderUsername { get; set; }

        public string SenderPhotoUrl { get; set; }

        #endregion


        #region Recipient
        public int RecipientId { get; set; }
        
        public string RecipientUsername { get; set; }
        
        public string RecipientPhotoUrl { get; set; }

        #endregion

        public string Content { get; set; }
        
        public DateTime DateTimeSent { get; set; }
        
        public DateTime? DateTimeRead { get; set; }
        


    }
}