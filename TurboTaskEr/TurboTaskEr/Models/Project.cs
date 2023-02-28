using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboTaskEr.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu!")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai multe de 100 de caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea este obligatorie!")]
        public string Description { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Deadline-ul este obligatoriu!")]
        public DateTime Deadline { get; set; }

        public string? UserId { get; set; } 

        public virtual ApplicationUser? User { get; set; }   

        public virtual ICollection<Task>? Tasks { get; set; }

        public virtual ICollection<Team>? Teams { get; set; }

    }
}
