using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TurboTaskEr.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu!")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aibe minim 5 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea este obligatorie!")]
        public String Description { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Deadline-ul este obligatoriu!")]
        public DateTime Deadline { get; set; }


        public int? ProjectId { get; set; }

        public virtual Project? Project { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        [Required(ErrorMessage = "Status-ul este obligatoriu!")]
        public int? StatusId { get; set; }

        public string? UserId { get; set; } 

        public virtual ApplicationUser? User { get; set; } 

        public virtual Status? Status { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Stat { get; set; } //dropdown-ul

        [NotMapped]
        public IEnumerable<SelectListItem>? UserInTeam { get; set; } //dropdown-ul

    }
}
