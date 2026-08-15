using System.ComponentModel.DataAnnotations;

namespace ModelValidationsExample.CustomValidators
{
    public class MinimumYearValidatorAttribute : ValidationAttribute
    {
        public int MinimumYear { get; set; } = 2000;
        public string DefaultErrorMessage { get; set; } = "Year should not be less than {0}";

        //parameterless constructor
        public MinimumYearValidatorAttribute()
        {
        }

        //parameterized constructor
        public MinimumYearValidatorAttribute(int minimumYear)
        {
            MinimumYear = minimumYear;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                //value={29/01/2004 12:00:00 ق.ظ}
                //date={29/01/2004 12:00:00 ق.ظ}
                DateTime date = (DateTime)value;
                //date.Year=2625
                if (date.Year >= MinimumYear)
                {
                    return new ValidationResult(string.Format(ErrorMessage ?? DefaultErrorMessage, MinimumYear));
                }
                else
                {
                    return ValidationResult.Success;
                }
            }

            return null;
        }
    }
}
