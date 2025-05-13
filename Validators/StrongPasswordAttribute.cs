using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace crewbackend.Validators
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var password = value as string;

            // If no password is provided, skip validation (considered valid)
            if (string.IsNullOrWhiteSpace(password))
                return true;

            var hasUpper = Regex.IsMatch(password, "[A-Z]");
            var hasLower = Regex.IsMatch(password, "[a-z]");
            var hasDigit = Regex.IsMatch(password, "[0-9]");
            var hasSpecial = Regex.IsMatch(password, "[^a-zA-Z0-9]");
            var hasMinLength = password.Length >= 8;

            return hasUpper && hasLower && hasDigit && hasSpecial && hasMinLength;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.";
        }
    }
}

