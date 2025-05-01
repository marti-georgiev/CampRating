using System.Collections.Generic;

namespace CampRating.Models
{
    public class HomeViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCampPlaces { get; set; }
        public int TotalReviews { get; set; }
        public IEnumerable<CampPlace> CampPlaces { get; set; }
    }
} 