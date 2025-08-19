using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class ChatController : Controller
{
    private readonly GeminiService _geminiService;

    public ChatController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Ask(string question)
    {
        var answer = await _geminiService.GetChatResponse(question);
        ViewBag.Question = question;
        ViewBag.Answer = answer;
        return View("Index");
    }
}
