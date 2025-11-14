namespace Fall2025_Project3_dllecorchick.Models
{
    public class ActorDetailsModel
    {
        public Actor Actor { get; set; } = default!;
        public List<(string Post, double Sentiment)>? Posts { get; set; }
        public double? AverageSentiment { get; set; }
    }
}
