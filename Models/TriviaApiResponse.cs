using Newtonsoft.Json;

namespace TwilioWhatsAppTriviaApp.Models;

public class TriviaApiResponse
{
    [JsonProperty("category")]
    public string Category { get; set; } = null!;

    [JsonProperty("correctAnswer")]
    public string CorrectAnswer { get; set; } = null!;

    [JsonProperty("incorrectAnswers")]
    public List<string> IncorrectAnswers { get; set; } = null!;

    [JsonProperty("question")]
    public TriviaQuestion Question { get; set; } = null!;

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("difficulty")]
    public string Difficulty { get; set; } = null!;
}

public class TriviaQuestion
{
    [JsonProperty("text")]
    public string Text { get; set; } = null!;
}
