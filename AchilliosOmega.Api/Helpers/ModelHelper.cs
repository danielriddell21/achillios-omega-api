using AchilliosOmega.Api.Models;
using Microsoft.ML;
using Newtonsoft.Json;
using System.Data;
using AchilliosOmega.Api.Interfaces;

namespace AchilliosOmega.Api.Helpers
{
    public class ModelHelper : IModelHelper
    {
        private string _modelFileName;
        private string _trainingDataFileName;

        private string? _modelFilePath;
        private string? _trainingDataFilePath;

        private string _trainingDataUrl;

        private readonly ILogger<ModelHelper> _logger;

        public ModelHelper(ILogger<ModelHelper> logger)
        {
            _logger = logger;

            _modelFileName = "model.zip";
            _trainingDataFileName = "intents.json";

            _trainingDataUrl = "https://drive.proton.me/urls/DEJXWFRN34#Kjvd0piZDQXS";
        }

        public ITransformer TrainChatbotModel(MLContext mlContext)
        {
            _trainingDataFilePath = DownloadDataset();
            var data = FormatDataset(_trainingDataFilePath);

            var dataView = mlContext.Data.LoadFromEnumerable(data);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Text")
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Intent"))
                .Append(mlContext.Transforms.NormalizeMinMax("Text"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedIntent"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Responses"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedResponse"))
                .Append(mlContext.Transforms.Text.ProduceWordBags("Text"))
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("Text"))
                .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Text"))
                .Append(mlContext.Transforms.Text.NormalizeText("Text"))
                .Append(mlContext.Transforms.Text.ProduceNgrams("Text"));

            var model = pipeline.Fit(dataView);

            _modelFilePath = SaveChatbotModel(mlContext, dataView, model);

            return model;
        }

        public string SaveChatbotModel(MLContext mlContext, IDataView data, ITransformer model)
        {
            mlContext.Model.Save(model, data.Schema, _modelFileName);
            return Path.Combine(Directory.GetCurrentDirectory(), _modelFileName); ;
        }

        public ITransformer LoadChatbotModel(MLContext mlContext)
        {
            if (File.Exists(_modelFilePath) || _modelFilePath == null)
                return TrainChatbotModel(mlContext);
            else
                return mlContext.Model.Load(_modelFilePath, out var modelSchema);
        }

        private string DownloadDataset()
        {
            using HttpClient client = new();
            using var response = client.GetAsync(_trainingDataUrl, HttpCompletionOption.ResponseHeadersRead).Result;
            using var fileStream = new FileStream(_trainingDataFileName, FileMode.Create, FileAccess.Write, FileShare.None);

            response.Content.CopyToAsync(fileStream).Wait();

            return fileStream.Name;
        }

        private IEnumerable<Message> FormatDataset(string datasetFilePath)
        {
            var datasetFile = File.ReadAllText(datasetFilePath);
            var dataset = JsonConvert.DeserializeObject<IEnumerable<DatasetRow>>(datasetFile);

            if (dataset != null)
            {
                return dataset
                    .SelectMany(datasetRow => datasetRow.Text
                    .SelectMany(text => datasetRow.Responses
                    .Select(response => new Message()
                        {
                            Intent = datasetRow.Intent,
                            Text = text,
                            Responses = response
                        })));
            } 
            else
            {
                throw new NullReferenceException("There was an issue trying to compile the dataset.");
            }
        }
    }
}
