using Microsoft.ML;

namespace AchilliosOmega.Api.Interfaces
{
    public interface IModelHelper
    {
        ITransformer BuildAndTrainModel(MLContext mlContext);
        string SaveModel(MLContext mlContext, IDataView data, ITransformer model);
        ITransformer LoadModel(MLContext mlContext);
    }
}
