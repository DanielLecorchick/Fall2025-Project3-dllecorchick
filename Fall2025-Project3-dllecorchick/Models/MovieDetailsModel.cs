namespace Fall2025_Project3_dllecorchick.Models
{
    public class MovieDetailsModel
    {
        public Movie Movie { get; set; } = default!;
        public List<(string Review, double Sentiment)>? Reviews { get; set; }
        public double? AverageSentiment { get; set; }
    }
}
