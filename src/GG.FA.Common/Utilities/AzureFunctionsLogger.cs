using System;
using System.Collections.Generic;
using System.Text;
using GG.FA.Common.Interfaces;

namespace GG.FA.Common.Utilities
{
    /// <summary>
    /// Azure Functions logger wrapper
    /// </summary>
    public class AzureFunctionLogger : ILogger
    {
        private static Microsoft.Azure.WebJobs.Host.TraceWriter _logger;

        public AzureFunctionLogger(Microsoft.Azure.WebJobs.Host.TraceWriter logger)
        {
            _logger = logger;
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warning(string message)
        {
            _logger.Warning(message);
        }
    }
}
