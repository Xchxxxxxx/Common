using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Exceptions
{
    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException() : base("Validation error occurred")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(Dictionary<string, string[]> errors) : base("Validation error occurred")
        {
            Errors = errors;
        }
    }

}
