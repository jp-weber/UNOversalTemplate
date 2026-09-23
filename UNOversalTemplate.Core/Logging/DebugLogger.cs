// Define DEBUG symbol to enable Debug.WriteLine to work even when Prism is compiled in Release-mode for Nuget-feed.
#define DEBUG
using System;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace UNOversal.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILoggerFacade"/> that logs into a message into the Debug.Listeners collection.
    /// </summary>
#if WINDOWS_UWP && NET10_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods)]
#endif
    public class DebugLogger : ILoggerFacade
    {
        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        public void Log(string message, Category category, Priority priority)
        {
            string messageToLog = string.Format(CultureInfo.InvariantCulture, "{1}: {2}. Priority: {3}. Timestamp:{0:u}.", DateTime.Now,
                                                category.ToString().ToUpper(), message, priority);

            Debug.WriteLine(messageToLog);
        }
    }
}
