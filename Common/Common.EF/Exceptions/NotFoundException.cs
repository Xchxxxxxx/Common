using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Resource not found") { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.") { }
    }
}
