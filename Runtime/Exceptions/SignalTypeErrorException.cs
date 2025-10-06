using System;

namespace AceLand.EventDriven.Exceptions
{
    public class SignalTypeErrorException : Exception
    {
        public SignalTypeErrorException(string message) : base(message)
        {
            
        }
    }
}