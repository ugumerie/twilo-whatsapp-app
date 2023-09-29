namespace TwilioWhatsAppTriviaApp.Models;

public class Question
{
    public string QuestionText { get; set; } = null!;
    public List<(string option, bool isCorrect)> Options { get; set; } = null!;
}
