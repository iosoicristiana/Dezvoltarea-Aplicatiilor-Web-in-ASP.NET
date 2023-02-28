using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboTaskEr.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }

        public virtual ICollection<Project>? Projects { get; set; }

        public virtual ICollection<Team>? Teams { get; set; }

        [Required(ErrorMessage = "Introduceti prenumele"), MaxLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Introduceti numele"), MaxLength(50)]
        public string LastName { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
