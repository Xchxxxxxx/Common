using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Service
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ValidationResult Success => new() { IsValid = true };

        public static ValidationResult Fail(string error)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { error }
            };
        }

        public static ValidationResult Fail(List<string> errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors
            };
        }

        public static ValidationResult Fail(params string[] errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }
    }
}
