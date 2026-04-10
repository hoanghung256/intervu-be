using System.ComponentModel.DataAnnotations;

namespace Intervu.Domain.Abstractions.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class MultipleOfAttribute : ValidationAttribute
    {
        public int Divisor { get; }

        public MultipleOfAttribute(int divisor)
        {
            if (divisor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(divisor), "Divisor must be greater than zero.");
            }

            Divisor = divisor;
            ErrorMessage = "The field {0} must be a multiple of {1}.";
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is int intValue)
            {
                return intValue % Divisor == 0;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, Divisor);
        }
    }
}
