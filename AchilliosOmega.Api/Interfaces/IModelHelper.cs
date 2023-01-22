using Microsoft.ML;

namespace AchilliosOmega.Api.Interfaces
{
    public interface IModelHelper
    {
        ITransformer TrainChatbotModel(MLContext mlContext);
        string SaveChatbotModel(MLContext mlContext, IDataView data, ITransformer model);
        ITransformer LoadChatbotModel(MLContext mlContext);
    }
}
