using System.Text.Json.Serialization;

namespace AuthorizationApp.Web.Models
{
    public class ValidationResult
    {
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; private set; }

         [JsonPropertyName("isValid")]
        public bool IsValid { get; private set; }

        public ValidationResult() {
            Errors = new Dictionary<string, string[]>();
            IsValid = true;
        }

        public static ValidationResult ValidationProblem(Dictionary<string, string[]> errors) => new ValidationResult() { Errors = errors, IsValid = false };
    }
}
