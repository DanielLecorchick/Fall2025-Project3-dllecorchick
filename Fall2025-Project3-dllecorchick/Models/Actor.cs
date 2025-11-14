using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dllecorchick.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Gender { get; set; } = default!;

        [Required]
        public int Age { get; set; }

        [Required]
        [Url]
        public string IMDBLink { get; set; } = default!;

        public byte[]? Photo { get; set; }

        public ICollection<ActorMovie>? ActorMovies { get; set; }
    }
}
