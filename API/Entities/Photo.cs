using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; } // public Id created and returned by Cloudinary after uploading

        public bool IsApproved { get; set; }
        
        

        public AppUser AppUser { get; set; }
        public int AppUserId { get; set; }
    }
}