using System.Globalization;
using System.Windows.Controls;

namespace Carrot.UI.ColorPicker.ValidationRules {

    internal class ColorNumericInputValidationRule : ValidationRule {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value == null || ((string)value)?.Length == 0) {
                return new ValidationResult(false, "Please enter a number");
            }

            return ValidationResult.ValidResult;
        }
    }
}