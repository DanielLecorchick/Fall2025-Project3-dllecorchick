using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dllecorchick.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        [Required]
        [Url]
        public string IMDBLink { get; set; } = default!;

        [Required]
        public string Genre { get; set; } = default!;

        [Required]
        public int Year { get; set; }

        public byte[]? Poster { get; set; }

        public ICollection<ActorMovie>? ActorMovies { get; set; }
    }
}
