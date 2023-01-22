using AchilliosOmega.Api.Models;
using Microsoft.ML;
using Newtonsoft.Json;
using System.Data;
using AchilliosOmega.Api.Interfaces;

namespace AchilliosOmega.Api.Helpers
{
    public class ModelHelper : IModelHelper
    {
        private readonly string _modelFileName;
        private readonly string _trainingDataFileName;

        private static string? _modelFilePath;
        private string? _trainingDataFilePath;

        private readonly string _trainingDataUrl;

        private readonly ILogger<ModelHelper> _logger;

        public ModelHelper(ILogger<ModelHelper> logger)
        {
            _logger = logger;

            _modelFileName = "model.zip";
            _trainingDataFileName = "intents.json";

            _trainingDataUrl = "https://pastebin.com/raw/F4tqKcp5";
        }

        public ITransformer BuildAndTrainModel(MLContext mlContext)
        {
            try
            {
                _trainingDataFilePath = DownloadDataset();
                var data = FormatDataset(_trainingDataFilePath);

                var dataView = mlContext.Data.LoadFromEnumerable(data);

                var pipeline = mlContext.Transforms.Text.FeaturizeText("Text", nameof(Message.Text))
                    .Append(mlContext.Transforms.NormalizeMinMax("Text"))
                    .Append(mlContext.Transforms.Conversion.MapValueToKey("Intent", nameof(Message.Intent)))
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("Intent"))
                    .Append(mlContext.Transforms.Conversion.MapValueToKey("Response", nameof(Message.Response)))
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("Response"))
                    .AppendCacheCheckpoint(mlContext);
                    //.Append(mlContext.Transforms.Text.ProduceWordBags("Text"))
                    //.Append(mlContext.Transforms.Text.TokenizeIntoWords("Text"))
                    //.Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Text"))
                    //.Append(mlContext.Transforms.Text.NormalizeText("Text"))
                    //.Append(mlContext.Transforms.Text.ProduceNgrams("Text"))

                var model = pipeline.Fit(dataView);

                _modelFilePath = SaveModel(mlContext, dataView, model);

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured training the model.", ex);
                throw;
            }
}

        public string SaveModel(MLContext mlContext, IDataView data, ITransformer model)
        {
            try
            {
                _logger.LogInformation("Saving Training Model");
                mlContext.Model.Save(model, data.Schema, _modelFileName);
                return Path.Combine(Directory.GetCurrentDirectory(), _modelFileName);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error occured saving the Training Model", ex);
                throw;
            }
        }

        public ITransformer LoadModel(MLContext mlContext)
        {
            try
            {
                if (File.Exists(_modelFilePath) || _modelFilePath == null)
                    return BuildAndTrainModel(mlContext);
                else
                    return mlContext.Model.Load(_modelFilePath, out var modelSchema);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error occured loading the Training Model", ex);
                throw;
            }
        }

        private string DownloadDataset()
        {
            try
            {
                using HttpClient client = new();
                using var response = client.GetAsync(_trainingDataUrl, HttpCompletionOption.ResponseHeadersRead).Result;
                using var fileStream = new FileStream(_trainingDataFileName, FileMode.Create, FileAccess.Write, FileShare.None);

                response.Content.CopyToAsync(fileStream).Wait();

                return fileStream.Name;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured downloading the dataset.", ex);
                throw;
            }
        }

        private IEnumerable<Message> FormatDataset(string datasetFilePath)
        {
            var datasetFile = File.ReadAllText(datasetFilePath);
            var dataset = JsonConvert.DeserializeObject<DatasetWrapper>(datasetFile);

            if (dataset != null)
            {
                return dataset.Intents
                    .SelectMany(datasetRow => datasetRow.Text
                    .SelectMany(text => datasetRow.Responses
                    .Select(response => new Message()
                        {
                            Intent = datasetRow.Intent,
                            Text = text,
                            Response = response
                        })));
            } 
            else
            {
                throw new NullReferenceException("Dataset file is not downloaded, or could not be found.");
            }
        }
    }
}
