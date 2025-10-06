using System;

namespace AceLand.EventDriven.Exceptions
{
    public class SignalNotFoundException : Exception
    {
        public SignalNotFoundException(string message) : base(message)
        {
            
        }
    }
}