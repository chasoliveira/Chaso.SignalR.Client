using System;

namespace Chaso.SignalR.Client
{
    public static class ExceptionExtension
    {
        public static string ToStringAllMessage(this Exception ex)
        {
            Exception exIn = ex;
            var message = "";
            
            while (exIn != null)
            {                
                message += $"Message: {exIn.Message}\r\n";
                exIn = exIn.InnerException;
            };
            return message;
        }
    }
}
