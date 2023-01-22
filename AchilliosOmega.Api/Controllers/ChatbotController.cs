using AchilliosOmega.Api.Interfaces;
using AchilliosOmega.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Newtonsoft.Json;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly ILogger<ChatbotController> _logger;

    private readonly MLContext _mlContext;
    private readonly IModelHelper _modelHelper;
    private ITransformer? _model;

    private static PredictionEngine<Message, ResponsePrediction>? _predictionEngine;

    public ChatbotController(ILogger<ChatbotController> logger, MLContext mlContext, IModelHelper modelHelper)
    {
        _logger = logger;
        _mlContext = mlContext;
        _modelHelper = modelHelper;

        if (_predictionEngine == null)
        {
            _model = _modelHelper.LoadChatbotModel(_mlContext);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<Message, ResponsePrediction>(_model);
        }

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("I think, therefore I am.");
    }

    [HttpPost]
    public IActionResult Post(Message input)
    {
        if (string.IsNullOrWhiteSpace(input.Text))
        {
            return StatusCode(500, "No message was sent.");
        }

        if (_predictionEngine != null)
        {
            var prediction = _predictionEngine.Predict(input);

            _logger.LogInformation($"Intent: {prediction.PredictedIntent}");
            _logger.LogInformation($"Response: {prediction.PredictedResponse}");
            _logger.LogInformation($"Confidence: {prediction.Score}");

            var output = new Message
            {
                Text = input.Text,
                Intent = prediction.PredictedIntent,
                Responses = prediction.PredictedResponse
            };

            return Ok(output);
        }
        else
        {
            return StatusCode(500, "Prediction Engine not loaded. ");
        }
    }
}
