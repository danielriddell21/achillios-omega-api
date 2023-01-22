namespace AchilliosOmega.Api.Models
{
    public class ResponsePrediction
    {
        public string? PredictedIntent { get; set; }
        public string? PredictedResponse { get; set; }
        public float[]? Score { get; set; }
    }
}
