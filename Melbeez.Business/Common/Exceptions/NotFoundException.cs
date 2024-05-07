using System;

namespace Melbeez.Business.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public readonly string errorMessage;

        public NotFoundException(string errorMessage)
        {
            this.errorMessage = errorMessage;
        }
    }
}