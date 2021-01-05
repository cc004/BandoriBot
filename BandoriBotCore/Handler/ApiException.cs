using System;
using System.Collections.Generic;
using System.Text;

namespace BandoriBot.Handler
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ApiException()
        {
        }
    }
}
