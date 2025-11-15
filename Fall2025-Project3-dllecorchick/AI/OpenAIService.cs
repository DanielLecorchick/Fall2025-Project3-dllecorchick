using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using VaderSharp2;

namespace Fall2025_Project3_dllecorchick.AI
{
    public class OpenAIService
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _deployment;

        public OpenAIService(IConfiguration config)
        {
            _endpoint = config["OpenAI:Endpoint"]!;
            _apiKey = config["OpenAI:ApiKey"]!;
            _deployment = config["OpenAI:Deployment"]!;
        }

        public async Task<List<(string Review, double Sentiment)>> GenerateReviewsAsync(
            string movieName, string movieYear, string movieDirector)
        {
            var client = new AzureOpenAIClient(new Uri(_endpoint), new ApiKeyCredential(_apiKey));
            var chatClient = client.GetChatClient(_deployment);

            string[] personas = {
                "a harsh critic", "a romance lover", "a comedy fan", "a thriller enthusiast", "a fantasy lover", "a sci-fi nerd", "an art-house critic", "a history buff", "a horror fan", "a casual moviegoer"
            };

            var messages = new ChatMessage[]
            {
                new SystemChatMessage($"You represent a group of {personas.Length} film critics with personalities: {string.Join(", ", personas)}. " +
                                      $"When you receive a question, respond as each member with responses separated by '|'. Do not label the reviewers."),
                new UserChatMessage($"How would you rate the movie {movieName} released in {movieYear}, directed by {movieDirector}, out of 10 in 150 words or less?")
            };

            var result = await chatClient.CompleteChatAsync(messages);
            string text = result.Value.Content[0].Text ?? string.Empty;

            string[] reviews = text.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var analyzer = new SentimentIntensityAnalyzer();
            var data = new List<(string Review, double Sentiment)>();

            foreach (var review in reviews)
            {
                var sentiment = analyzer.PolarityScores(review);
                data.Add((review, sentiment.Compound));
            }

            return data;
        }

        public async Task<List<(string Post, double Sentiment)>> GenerateActorPostsAsync(string actorName)
        {
            var client = new AzureOpenAIClient(new Uri(_endpoint), new ApiKeyCredential(_apiKey));
            var chatClient = client.GetChatClient(_deployment);

            var messages = new ChatMessage[]
            {
        new SystemChatMessage(
            "You are simulating social media fans reacting to a celebrity. " +
            "Generate 20 short, tweet-style posts about the actor. " +
            "Each post must be separated by '|' and written in an authentic human tone (some funny, some critical, some emotional, etc.). " +
            "Keep each post under 25 words. Do not number or label them."
        ),
        new UserChatMessage($"Generate 20 unique tweets about the actor {actorName}.")
            };

            var result = await chatClient.CompleteChatAsync(messages);
            string text = result.Value.Content[0].Text ?? string.Empty;

            string[] posts = text.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var analyzer = new SentimentIntensityAnalyzer();
            var data = new List<(string Post, double Sentiment)>();

            foreach (var post in posts)
            {
                var sentiment = analyzer.PolarityScores(post);
                data.Add((post, sentiment.Compound));
            }

            while (data.Count < 20)
                data.Add(("No post generated.", 0.0));

            return data;
        }
    }
}