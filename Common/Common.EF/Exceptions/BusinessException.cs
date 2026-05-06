using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Exceptions
{
    public class BusinessException : Exception
    {
        public string Code { get; }

        public BusinessException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
