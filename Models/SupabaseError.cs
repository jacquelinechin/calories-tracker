using System.Text.Json.Serialization;

namespace CaloriesTracker.Models
{
    public class SupabaseError
    {
        public int Code { get; set; }

        [JsonPropertyName("error_code")]
        public string ErrorCode { get; set; } = string.Empty;

        [JsonPropertyName("msg")]
        public string Message { get; set; } = string.Empty;
    }
}
