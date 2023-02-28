using System.ComponentModel.DataAnnotations;

namespace TurboTaskEr.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu!")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int? TaskId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Task? Task { get; set; }
    }
}
