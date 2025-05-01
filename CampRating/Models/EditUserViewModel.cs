using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampRating.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public List<string> CurrentRoles { get; set; }
        public List<string> AllRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
} 