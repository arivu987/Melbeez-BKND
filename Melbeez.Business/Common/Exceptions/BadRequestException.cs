using System;

namespace Melbeez.Business.Common.Exceptions
{
    public class BadRequestException : Exception
    {
        public readonly string errorMessage;

        public BadRequestException(string errorMessage)
        {
            this.errorMessage = errorMessage;
        }
    }
}
