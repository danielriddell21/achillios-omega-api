using Microsoft.ML.Data;

namespace AchilliosOmega.Api.Models
{
    public class Message
    {
        [ColumnName("Text")]
        public string? Text { get; set; }
        [ColumnName("Intent")]
        public string? Intent { get; set; }
        [ColumnName("Response")]
        public string? Response { get; set; }
    }
}
