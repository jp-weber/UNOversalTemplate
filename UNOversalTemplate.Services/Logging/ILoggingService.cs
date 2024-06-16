using System;
using System.Threading.Tasks;

namespace UNOversal.Services.Logging
{
    public interface ILoggingService
    {
        Task Log(string message, LoggingPreferEnum loggingPreferEnum);
        Task LogException(Exception exc, LoggingPreferEnum loggingPreferEnum);
    }
}
