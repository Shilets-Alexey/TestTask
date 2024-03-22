using System.Text.Json.Serialization;

namespace AuthorizationApp.Server.Models
{
    public class ServiceResult<TKey>
    {

        public TKey Result { get; private set; }

        public ValidationResult ValidationResult { get; private set; }

        private ServiceResult() {}

        public static ServiceResult<TKey> Create() => new ServiceResult<TKey>() { Result = Activator.CreateInstance<TKey>(), ValidationResult = new ValidationResult() };

        public static ServiceResult<TKey> Create(TKey data) => new ServiceResult<TKey>() { Result = data, ValidationResult = new ValidationResult()};

        public static ServiceResult<TKey> Create(ValidationResult validationResult) => new ServiceResult<TKey>() { ValidationResult = validationResult, Result = Activator.CreateInstance<TKey>() };

        public static ServiceResult<TKey> Create(TKey data, ValidationResult validationResult) => new ServiceResult<TKey>() { Result = data, ValidationResult = validationResult };

    }
}
