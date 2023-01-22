using System.Collections.Generic;

namespace AchilliosOmega.Api.Models
{
    public class DatasetRow
    {
        public string Intent { get; set; }
        public IEnumerable<string> Text { get; set; }
        public IEnumerable<string> Responses { get; set; }
    }
}
