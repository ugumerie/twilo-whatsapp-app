using TwilioWhatsAppTriviaApp.Models;

namespace TwilioWhatsAppTriviaApp.Services;

public class TriviaService
{
    private const string TheTriviaApiUrl = @"https://the-trivia-api.com/api/questions?limit=3";
    private readonly IHttpClientFactory _httpClientFactory;

    public TriviaService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<TriviaApiResponse>?> GetTriviaAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("trivia");
        return await httpClient.GetFromJsonAsync<IEnumerable<TriviaApiResponse>>("questions?limit=3");
    }

    public List<Question> ConvertTriviaToQuestions(IEnumerable<TriviaApiResponse> triviaQuestions)
    {
        List<Question> newQuestions = new();
        foreach (var question in triviaQuestions)
        {
            var options = new List<(string option, bool isCorrect)>
            {
                (question.CorrectAnswer, true),
                (question.IncorrectAnswers[0], false),
                (question.IncorrectAnswers[1], false),
                (question.IncorrectAnswers[2], false)
            };

            // Randomize the options
            Random random = Random.Shared;
            options = options.OrderBy(_ => random.Next()).ToList();

            newQuestions.Add(new Question
            {
                QuestionText = question.Question.Text,
                Options = options
            });
        }

        return newQuestions;
    }
}
