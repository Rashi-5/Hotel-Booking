using HotelBookingSystem.Models.Chatbot;
using HotelBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models.Prediction;

namespace HotelBookingSystem.Controllers
{
    public class ChatbotController : Controller
{
    private readonly ChatbotService _chatbotService;

    public ChatbotController(ChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost]
    public JsonResult Ask([FromBody] ChatMessage model)
    {
        model.BotResponse = _chatbotService.GetBotResponse(model.UserMessage);
        return Json(new { botResponse = model.BotResponse });
    }
}


    public class ReportController : Controller
{
    private readonly PredictionService _predictionService;

    public ReportController(PredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    public IActionResult Prediction()
    {
        var report = _predictionService.GeneratePredictionReport();
        return View(report);
    }
}

}
