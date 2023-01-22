using Microsoft.ML.Data;

namespace AchilliosOmega.Api.Models
{
    public class ResponsePrediction
    {
        [ColumnName("Intent")]
        public string? PredictedIntent { get; set; }
        [ColumnName("Response")]
        public string? PredictedResponse { get; set; }
        [ColumnName("Score")]
        public float[]? Score { get; set; }
    }
}
