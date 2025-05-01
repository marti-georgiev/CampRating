using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampRating.Models
{
    /// <summary>
    /// Модел за място за къмпингуване
    /// </summary>
    public class CampPlace
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(64, ErrorMessage = "Името не може да бъде по-дълго от 64 символа")]
        [Display(Name = "Име")]
        public string Name { get; set; } = null!;
        
        [Required(ErrorMessage = "Описанието е задължително")]
        [StringLength(255, ErrorMessage = "Описанието не може да бъде по-дълго от 255 символа")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = null!;
        
        [Required(ErrorMessage = "Географската ширина е задължителна")]
        [Display(Name = "Географска ширина")]
        public double Latitude { get; set; }
        
        [Required(ErrorMessage = "Географската дължина е задължителна")]
        [Display(Name = "Географска дължина")]
        public double Longitude { get; set; }
        
        [StringLength(2000)]
        [Display(Name = "Снимка")]
        public string? Photo { get; set; }

        [Display(Name = "Дата на създаване")]
        public DateTime DateCreated { get; set; } = DateTime.Now;
        
        [Display(Name = "Дата на промяна")]
        public DateTime? DateModified { get; set; }

        // Връзка с потребителя, който е създал мястото
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Колекция от ревюта за това място
        public ICollection<Review>? Reviews { get; set; }
    }
} 