using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampRating.Models
{
    /// <summary>
    /// Модел за ревю на място за къмпингуване
    /// </summary>
    public class Review
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Мястото за къмпингуване е задължително")]
        [Display(Name = "Място за къмпингуване")]
        public int CampPlaceId { get; set; }
        public CampPlace? CampPlace { get; set; }
        
        [Required(ErrorMessage = "Потребителят е задължителен")]
        [Display(Name = "Потребител")]
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }
        
        [Required(ErrorMessage = "Рейтингът е задължителен")]
        [Range(1, 5, ErrorMessage = "Рейтингът трябва да бъде между 1 и 5")]
        [Display(Name = "Рейтинг")]
        public int Rating { get; set; }
        
        [Required(ErrorMessage = "Коментарът е задължителен")]
        [StringLength(500, ErrorMessage = "Коментарът не може да бъде по-дълъг от 500 символа")]
        [Display(Name = "Коментар")]
        public string Comment { get; set; } = null!;
        
        [Display(Name = "Дата на създаване")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Дата на промяна")]
        public DateTime? DateModified { get; set; }
    }
} 