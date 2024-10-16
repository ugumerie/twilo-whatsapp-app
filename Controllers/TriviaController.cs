using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Messaging;
using TwilioWhatsAppTriviaApp.Models;
using TwilioWhatsAppTriviaApp.Services;

namespace TwilioWhatsAppTriviaApp.Controllers;

[Route("[controller]")]
[ApiController]
public class TriviaController : TwilioController
{
    private const string SessionKeyIsGameOn = "IsGameOn";
    private const string SessionKeyScore = "Score";
    private const string SessionKeyCurrentQuestionIndex = "CurrentQuestionIndex";
    private const string SessionKeyTotalQuestions = "TotalQuestions";
    private const string SessionKeyQuestions = "Questions";

    private static readonly string[] StartCommands = ["START", "S"];
    private static readonly string[] OptionValues = ["A", "B", "C", "D"];

    private readonly TriviaService _triviaService;

    public TriviaController(TriviaService triviaService)
    {
        _triviaService = triviaService;
    }

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var response = new MessagingResponse();
        var form = await Request.ReadFormAsync();
        var body = form["Body"].ToString().ToUpper().Trim();

        await HttpContext.Session.LoadAsync();
        bool isGameOn = Convert.ToBoolean(HttpContext.Session.GetString(SessionKeyIsGameOn));
        int currentQuestionIndex = HttpContext.Session.GetInt32(SessionKeyCurrentQuestionIndex) ?? 0;
        int totalQuestions = HttpContext.Session.GetInt32(SessionKeyTotalQuestions) ?? 0;

        if (StartCommands.Contains(body) && !isGameOn)
        {
            await StartGameAsync();
            HttpContext.Session.SetString(SessionKeyIsGameOn, "true");

            response.Message(PresentQuestionWithOptions(currentQuestionIndex));
            return TwiML(response);
        }

        if (OptionValues.Contains(body) && isGameOn)
        {
            var result = ProcessUserAnswer(body, currentQuestionIndex);
            response.Message(result);

            currentQuestionIndex++;
            if (currentQuestionIndex <= totalQuestions - 1)
            {
                HttpContext.Session.SetInt32(SessionKeyCurrentQuestionIndex, currentQuestionIndex);
                response.Append(new Message(PresentQuestionWithOptions(currentQuestionIndex)));
            }
            else
            {
                response.Append(new Message(EndTrivia()));
            }

            return TwiML(response);
        }

        response.Message(!isGameOn ? 
            "*Hello! Send 'Start' or 'S' to play game*" : "*Invalid Input! Send a correct option 'A', 'B', 'C' or 'D'*");

        return TwiML(response);
    }

    private void AddNewQuestionsToSession(List<Question> questions)
         => HttpContext.Session.SetString(SessionKeyQuestions, JsonConvert.SerializeObject(questions));

    private Question RetrieveQuestionFromSession(int questionIndex)
    {
        var questionsFromSession = HttpContext.Session.GetString(SessionKeyQuestions);
        return JsonConvert.DeserializeObject<List<Question>>(questionsFromSession!)![questionIndex];
    }

    private async Task StartGameAsync()
    {
        if (HttpContext.Session.GetString(SessionKeyQuestions) is not null)
        {
            HttpContext.Session.Remove(SessionKeyQuestions);
        }

        var triviaQuestions = await _triviaService.GetTriviaAsync();
        if (triviaQuestions is null) return;

        var questions = _triviaService.ConvertTriviaToQuestions(triviaQuestions);
        AddNewQuestionsToSession(questions);
        HttpContext.Session.SetInt32(SessionKeyTotalQuestions, questions.Count);
    }

    private string ProcessUserAnswer(string userAnswer, int questionIndex)
    {
        int score = HttpContext.Session.GetInt32(SessionKeyScore) ?? 0;
        var question = RetrieveQuestionFromSession(questionIndex);

        bool optionIsCorrect = userAnswer switch
        {
            "A" => question.Options[0].isCorrect,
            "B" => question.Options[1].isCorrect,
            "C" => question.Options[2].isCorrect,
            "D" => question.Options[3].isCorrect,
            _ => false
        };

        if (optionIsCorrect)
        {
            score++;
            HttpContext.Session.SetInt32(SessionKeyScore, score);
        }

        return optionIsCorrect ? "_Correct ✅_" : $"_Incorrect ❌ Correct answer is {question.Options
            .Find(o => o.isCorrect).option.TrimEnd()}_";
    }

    private string PresentQuestionWithOptions(int questionIndex)
    {
        var question = RetrieveQuestionFromSession(questionIndex);
        return $"""
                {questionIndex + 1}. {question.QuestionText}
                {OptionValues[0]}. {question.Options[0].option}
                {OptionValues[1]}. {question.Options[1].option}
                {OptionValues[2]}. {question.Options[2].option}
                {OptionValues[3]}. {question.Options[3].option}
            """;
    }

    private string EndTrivia()
    {
        var score = HttpContext.Session.GetInt32(SessionKeyScore) ?? 0;
        var totalQuestions = HttpContext.Session.GetInt32(SessionKeyTotalQuestions) ?? 0;

        var userResult = $"""
                Thanks for playing! 😊
                You answered {score} out of {totalQuestions} questions correctly.
                
                To play again, send 'Start' or 'S'
                """;
        HttpContext.Session.Clear();
        return userResult;
    }
}
