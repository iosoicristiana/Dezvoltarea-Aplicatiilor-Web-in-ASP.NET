using System.ComponentModel.DataAnnotations;

namespace TurboTaskEr.Models
{
    public class Status
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Statusul este obligatoriu!")]
        public string StatusName { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
