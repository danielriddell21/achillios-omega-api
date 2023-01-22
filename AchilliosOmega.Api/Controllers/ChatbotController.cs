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

    private static PredictionEngine<Message, ResponsePrediction>? _predictionEngine;

    public ChatbotController(ILogger<ChatbotController> logger, MLContext mlContext, IModelHelper modelHelper)
    {
        try
        {
            _logger = logger;
            _mlContext = mlContext;
            _modelHelper = modelHelper;

            if (_predictionEngine == null)
            {
                var _model = _modelHelper.LoadModel(_mlContext);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<Message, ResponsePrediction>(_model);
            }

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        } 
        catch(Exception ex)
        {
            _logger?.LogError("Error occured setting up the api.", ex);
            throw;
        }
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("I think, therefore I am.");
    }

    [HttpPost]
    public IActionResult Post([FromBody] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return StatusCode(500, "No message was sent.");
        }

        if (_predictionEngine != null)
        {
            var prediction = _predictionEngine.Predict(new Message { Text = text });

            if (!string.IsNullOrWhiteSpace(prediction.PredictedIntent) || !string.IsNullOrWhiteSpace(prediction.PredictedResponse))
            {
                _logger.LogInformation($"Intent: {prediction.PredictedIntent}");
                _logger.LogInformation($"Response: {prediction.PredictedResponse}");
                _logger.LogInformation($"Confidence: {prediction.Score}");

                var output = new Message
                {
                    Text = text,
                    Intent = prediction.PredictedIntent,
                    Response = prediction.PredictedResponse
                };

                return Ok(output);
            }
            else
            {
                return StatusCode(500, "An error has occured when responding.");
            }
        }
        else
        {
            return StatusCode(500, "Prediction Engine not loaded.");
        }
    }
}
