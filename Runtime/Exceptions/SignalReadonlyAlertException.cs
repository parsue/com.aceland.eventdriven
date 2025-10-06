using System;

namespace AceLand.EventDriven.Exceptions
{
    public class SignalReadonlyAlertException : Exception
    {
        public SignalReadonlyAlertException(string message) : base(message)
        {
            
        }
    }
}